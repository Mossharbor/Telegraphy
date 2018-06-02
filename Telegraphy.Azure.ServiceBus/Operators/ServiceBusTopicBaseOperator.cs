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
    using Telegraphy.Net;
    using Telegraphy.Net.Exceptions;

    public class ServiceBusTopicBaseOperator<MsgType> : IOperator where MsgType : class
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private int maxDequeueCount = 1;
        private ServiceBusTopicDeliverer ServiceBusMsgSender = null;
        private ServiceBusTopicReciever ServiceBusMsgReciever = null;
        ControlMessages.HangUp hangUp = null;
        ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();

        internal ServiceBusTopicBaseOperator(ServiceBusTopicDeliverer serviceBusMsgSender)
        {
            this.ServiceBusMsgSender = serviceBusMsgSender;
            this.ID = 0;
        }

        internal ServiceBusTopicBaseOperator(ILocalSwitchboard switchboard, ServiceBusTopicReciever serviceBusMsgReciever, int maxDequeueCount)
        {
            this.maxDequeueCount = maxDequeueCount;
            this.Switchboards.Add(switchboard);
            switchboard.Operator = this;
            this.ServiceBusMsgReciever = serviceBusMsgReciever;
            this.ID = 0;
            switchboard.Operator = this;

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
            if (typeof(MsgType) == typeof(string))
            {
                msg = Encoding.UTF8.GetString(sbMessage.Body).ToActorMessage();
            }
            else if (typeof(MsgType) == typeof(byte[]))
            {
                msg = sbMessage.Body.ToActorMessage();
            }
            else
            {
                byte[] msgBytes = sbMessage.Body;
                var t = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(msgBytes));
                msg = t.Result as IActorMessage;
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

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }

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
                foreach (var switchBoard in this.Switchboards)
                    switchBoard.Disable();
                return;
            }

            System.Diagnostics.Debug.Assert(null != ServiceBusMsgSender);
            Message message = SerializeAndSend(msg, this.ServiceBusMsgSender);

            if (null != msg.Status)
                msg.Status?.SetResult(new ServiceBusMessage(message));
        }

        internal static Message SerializeAndSend(IActorMessage msg, ServiceBusTopicDeliverer queue)
        {
            Message message = ServiceBusQueueBaseOperator<MsgType>.BuildMessage(msg);
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

            foreach (var switchBoard in this.Switchboards)
                switchBoard.Disable();
        }
        
        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
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
