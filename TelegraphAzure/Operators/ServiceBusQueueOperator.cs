using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueOperator : IOperator
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private bool recievingOnly = false;
        private ServiceBusQueue queue;
        private ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();
        private ControlMessages.HangUp hangUp = null;
        private int maxDequeueCount;
        private int maxConcurrentCalls = 1;

        internal ServiceBusQueueOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : this(queue, true)
        {
            this.Switchboard = switchBoard;
            this.Switchboard.Operator = this;
            this.maxDequeueCount = maxDequeueCount;
        }

        internal ServiceBusQueueOperator(ServiceBusQueue queue) :this(queue, false)
        {

        }

        internal ServiceBusQueueOperator(ServiceBusQueue queue, bool recievingOnly)
        {
            this.queue = queue;
            this.recievingOnly = recievingOnly;
            
            if (recievingOnly)
                RegisterOnMessageHandlerAndReceiveMessages();
        }

        void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(HandleExceptions)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = maxConcurrentCalls,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            queue.RegisterMessageHandler(ProcessMessages, messageHandlerOptions);
        }

        private Task ProcessMessages(Message sbMessage, CancellationToken token)
        {
            if (sbMessage.SystemProperties.DeliveryCount >= maxDequeueCount)
            {
                return queue.AbandonAsync(sbMessage.SystemProperties.LockToken);
            }

            // Process the message
            byte[] msgBytes = sbMessage.Body;
            var t = Telegraph.Instance.Ask(new DeSerializeMessage(msgBytes));
            IActorMessage msg = t.Result as IActorMessage;

            msgQueue.Enqueue(msg);

            if (null == msg.Status)
                msg.Status = new TaskCompletionSource<IActorMessage>();

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            msg.Status.Task.ContinueWith(p =>
            {
                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
                // to avoid unnecessary exceptions.

                if (token.IsCancellationRequested)
                    queue.AbandonAsync(sbMessage.SystemProperties.LockToken).Wait();
                else
                    queue.CompleteAsync(sbMessage.SystemProperties.LockToken).Wait();
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

        public ulong Count { get { return (ulong)queue.ApproximateMessageCount; } }

        public ILocalSwitchboard Switchboard { get; set; }

        public void AddMessage(IActorMessage msg)
        {
            if (msg is ControlMessages.HangUp)
            {
                if (null != hangUp)
                    return;

                if (null != queue)
                {
                    this.queue.CloseAsync().Wait();
                    if (null != msg.Status && !msg.Status.Task.IsCompleted)
                        msg.Status.SetResult(msg);
                    return;
                }

                this.queue.CloseAsync().Wait();
                hangUp = (msg as ControlMessages.HangUp);
                this.Switchboard.Disable();
                return;
            }

            if (this.recievingOnly)
                throw new OperatorCannotSendMessagesException();

            System.Diagnostics.Debug.Assert(null != queue);
            var serializeTask = Telegraph.Instance.Ask(new SerializeMessage(msg));
            byte[] msgBytes = (serializeTask.Result.ProcessingResult as byte[]);

            var message = new Message(msgBytes);

            if (msg is IServiceBusPropertiesProvider)
            {
                message.ScheduledEnqueueTimeUtc = (msg as IServiceBusPropertiesProvider).ScheduledEnqueueTimeUtc.HasValue ? (msg as IServiceBusPropertiesProvider).ScheduledEnqueueTimeUtc.Value : message.ScheduledEnqueueTimeUtc;
                message.TimeToLive = (msg as IServiceBusPropertiesProvider).TimeToLive.HasValue ? (msg as IServiceBusPropertiesProvider).TimeToLive.Value : message.TimeToLive;
                message.ContentType = (msg as IServiceBusPropertiesProvider).ContentType ?? message.ContentType;
                message.Label = (msg as IServiceBusPropertiesProvider).Label ?? message.Label;
                message.CorrelationId = (msg as IServiceBusPropertiesProvider).CorrelationId ?? message.CorrelationId;
                message.ReplyToSessionId = (msg as IServiceBusPropertiesProvider).ReplyToSessionId ?? message.ReplyToSessionId;
                message.SessionId = (msg as IServiceBusPropertiesProvider).SessionId ?? message.SessionId;
                message.MessageId = (msg as IServiceBusPropertiesProvider).MessageId ?? message.MessageId;
                if (null != (msg as IServiceBusPropertiesProvider).UserProperties)
                {
                    foreach (var t in (msg as IServiceBusPropertiesProvider).UserProperties)
                        message.UserProperties.Add(t);
                }
            }

            if ((message is IActorMessageIdentifier) && String.IsNullOrWhiteSpace(message.MessageId))
            {
                message.MessageId = (msg as IActorMessageIdentifier).Id;
            }

            this.queue.SendAsync(message).Wait();

            if(null != msg.Status)
                msg.Status?.SetResult(new ServiceBusMessage(message));
        }

        public IActorMessage GetMessage()
        {
            IActorMessage msg = null;
            if (!msgQueue.TryDequeue(out msg))
                return null;

            return msg;
        }

        public bool IsAlive()
        {
            if (queue.IsClosedOrClosing)
                return false;

            if (recievingOnly)
                return this.Switchboard.IsDisabled();
            return true;
        }

        public void Kill()
        {
            queue.CloseAsync().Wait();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            if (null == this.Switchboard && !recievingOnly)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
            if (null == this.Switchboard && !recievingOnly)
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

        public virtual bool WaitTillEmpty(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start) < timeout && (0 != this.msgQueue.Count || 0 != this.queue.ApproximateMessageCount))
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
