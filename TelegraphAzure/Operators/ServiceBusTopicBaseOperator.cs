using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using System.Collections.Concurrent;
    using System.Threading;
    using Telegraphy.Azure.Exceptions;
    using Telegraphy.Net;

    public class ServiceBusTopicBaseOperator : IOperator
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private int maxDequeueCount = 1;
        private ServiceBusTopicDeliverer ServiceBusMsgSender = null;
        private ServiceBusTopicReciever ServiceBusMsgReciever = null;
        ControlMessages.HangUp hangUp = null;
        ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();
        MessageSource messageSource = MessageSource.EntireIActor;

        internal ServiceBusTopicBaseOperator(ServiceBusTopicDeliverer serviceBusMsgSender, MessageSource messageSource = MessageSource.EntireIActor)
        {
            this.messageSource = messageSource;
            this.ServiceBusMsgSender = serviceBusMsgSender;
            this.ID = 0;
        }

        internal ServiceBusTopicBaseOperator(ILocalSwitchboard switchboard, ServiceBusTopicReciever serviceBusMsgReciever, int maxDequeueCount, MessageSource messageSource = MessageSource.EntireIActor)
        {
            this.messageSource = messageSource;
            this.maxDequeueCount = maxDequeueCount;
            this.Switchboard = switchboard;
            this.ServiceBusMsgReciever = serviceBusMsgReciever;
            this.ID = 0;
            if (null != Switchboard)
                Switchboard.Operator = this;

            if (null == switchboard)
                throw new SwitchBoardNeededWhenRecievingMessagesException();

            MessageHandlerOptions options = new MessageHandlerOptions(HandleExceptions);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = (int)switchboard.Concurrency;
            
            this.ServiceBusMsgReciever.RegisterMessageHandler(ListenForMessages, options);
        }
        
        private Task ListenForMessages(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken token)
        {
            if (sbMessage.SystemProperties.DeliveryCount > maxDequeueCount)
            {
                return this.ServiceBusMsgReciever.DeadLetterAsync(sbMessage.SystemProperties.LockToken);
            }

            IActorMessage msg = null;
            switch (this.messageSource)
            {
                case MessageSource.EntireIActor:
                    {
                        byte[] msgBytes = sbMessage.Body;
                        var t = Telegraph.Instance.Ask(new DeSerializeMessage(msgBytes));
                        msg = t.Result as IActorMessage;
                    }
                    break;

                case MessageSource.ByteArrayMessage:
                    msg = sbMessage.Body.ToActorMessage(); break;
                case MessageSource.StringMessage:
                    msg = Encoding.UTF8.GetString(sbMessage.Body).ToActorMessage(); break;
            }

            msgQueue.Enqueue(msg);

            if (null == msg.Status)
                msg.Status = new TaskCompletionSource<IActorMessage>();

            msg.Status.Task.ContinueWith(p =>
            {
                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
                // to avoid unnecessary exceptions.
                if (token.IsCancellationRequested)
                    this.ServiceBusMsgReciever.AbandonAsync(sbMessage.SystemProperties.LockToken);
                else
                    this.ServiceBusMsgReciever.CompleteAsync(sbMessage.SystemProperties.LockToken);
            });

            return msg.Status.Task;
        }
        
        private Task HandleExceptions(ExceptionReceivedEventArgs eargs)
        {
            return Task.Factory.StartNew(() =>
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, eargs.Exception, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
            });
        }

        #region IOperator

        public long ID { get; set; }

        public ulong Count { get { return (ulong)msgQueue.Count; } }

        public ILocalSwitchboard Switchboard { get; set; }

        public void AddMessage(IActorMessage msg)
        {
            if (msg is ControlMessages.HangUp)
            {
                if (null != hangUp)
                    return;

                if (null != ServiceBusMsgSender)
                {
                    ServiceBusMsgSender.CloseAsync().Wait();
                    if (null != msg.Status && !msg.Status.Task.IsCompleted)
                        msg.Status.SetResult(msg);
                    return;
                }

                ServiceBusMsgReciever.CloseAsync().Wait();
                hangUp = (msg as ControlMessages.HangUp);
                this.Switchboard.Disable();
                return;
            }

            System.Diagnostics.Debug.Assert(null != ServiceBusMsgSender);
            Message message = SerializeAndSend(msg, this.ServiceBusMsgSender, this.messageSource);

            if (null != msg.Status)
                msg.Status?.SetResult(new ServiceBusMessage(message));
        }

        internal static Message SerializeAndSend<T>(T msg, ServiceBusTopicDeliverer queue, MessageSource messageSource) where T : class, IActorMessage
        {
            Message message = ServiceBusQueueBaseOperator.BuildMessage(msg, messageSource);

            queue.SendAsync(message).Wait();
            return message;
        }

        public IActorMessage GetMessage()
        {
            System.Diagnostics.Debug.Assert(null != ServiceBusMsgReciever);

            IActorMessage msg = null;
            if (!msgQueue.TryDequeue(out msg))
                return null;
            
            return msg;
        }

        public bool IsAlive()
        {
            if (null == ServiceBusMsgReciever ? this.ServiceBusMsgSender.IsClosedOrClosing : this.ServiceBusMsgReciever.IsClosedOrClosing)
                return false;
            
            return this.Switchboard.IsDisabled();
        }

        public void Kill()
        {
            if (null != this.ServiceBusMsgSender)
            {
                this.ServiceBusMsgSender.CloseAsync().Wait();
            }
            else
            {
                this.ServiceBusMsgReciever.CloseAsync().Wait();
            }

            this.Switchboard.Disable();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            if (null == this.Switchboard && null != ServiceBusMsgReciever)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
            if (null == this.Switchboard && null != ServiceBusMsgReciever)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T, K>(factory);
        }

        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            this.Switchboard.Register(exceptionType, handler);
        }

        public uint ApproximateMessageCount
        {
            get
            {
                if (null != this.ServiceBusMsgSender)
                    return this.ServiceBusMsgSender.ApproximateMessageCount;
                else
                    return this.ServiceBusMsgReciever.ApproximateMessageCount;
            }
        }

        public virtual bool WaitTillEmpty(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start) < timeout && (0 != this.msgQueue.Count || 0 != this.ApproximateMessageCount))
            {
                System.Threading.Thread.Sleep(1000);
            }

            return ((DateTime.Now - start) <= timeout);
        }
        #endregion

        #region IActor

        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            AddMessage(msg);
            return true;
        }
        #endregion
    }
}
