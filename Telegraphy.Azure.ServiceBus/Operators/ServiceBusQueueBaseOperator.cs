using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueBaseOperator<MsgType> : IOperator where MsgType : class
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private bool recievingOnly = false;
        private ServiceBusQueue queue;
        private ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();
        private ControlMessages.HangUp hangUp = null;
        private int maxDequeueCount;
        private int maxConcurrentCalls = 1;

        internal ServiceBusQueueBaseOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : this(queue, true)
        {
            this.Switchboards.Add(switchBoard);
            switchBoard.Operator = this;
            this.maxDequeueCount = maxDequeueCount;
        }

        internal ServiceBusQueueBaseOperator(ServiceBusQueue queue) 
            :this(queue, false)
        {

        }

        private ServiceBusQueueBaseOperator(ServiceBusQueue queue, bool recievingOnly)
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
            queue.RegisterMessageHandler(RecieveMessages, messageHandlerOptions);
        }

        private Task RecieveMessages(Message sbMessage, CancellationToken token)
        {
            if (sbMessage.SystemProperties.DeliveryCount >= maxDequeueCount)
            {
                return queue.DeadLetterAsync(sbMessage.SystemProperties.LockToken);
            }

            IActorMessage msg = null;
            if (typeof(MsgType) == typeof(byte[]))
                msg = sbMessage.Body.ToActorMessage(); 
            else if (typeof(MsgType) == typeof(string))
                msg = Encoding.UTF8.GetString(sbMessage.Body).ToActorMessage();
            else
            {
                byte[] msgBytes = sbMessage.Body;
                var t = Telegraph.Instance.Ask(new DeserializeMessage<MsgType>(msgBytes));
                msg = t.Result as IActorMessage;
            }
            
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

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }

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
                foreach(var switchboard in this.Switchboards)
                    switchboard.Disable();
                return;
            }

            if (this.recievingOnly)
                throw new OperatorCannotSendMessagesException();

            System.Diagnostics.Debug.Assert(null != queue);
            Message message = SerializeAndSend(msg, this.queue);

            if (null != msg.Status)
                msg.Status?.SetResult(new ServiceBusMessage(message));
        }

        internal static Message BuildMessage(IActorMessage msg)
        {
            byte[] msgBytes = TempSerialization.GetBytes<MsgType>(msg);

            System.Diagnostics.Debug.WriteLine("Serializing Byte Count:" + msgBytes.Count());
            var message = new Message(msgBytes);

            if (msg is IServiceBusMessagePropertiesProvider)
            {
                message.ScheduledEnqueueTimeUtc = (msg as IServiceBusMessagePropertiesProvider).ScheduledEnqueueTimeUtc.HasValue ? (msg as IServiceBusMessagePropertiesProvider).ScheduledEnqueueTimeUtc.Value : message.ScheduledEnqueueTimeUtc;
                message.TimeToLive = (msg as IServiceBusMessagePropertiesProvider).TimeToLive.HasValue ? (msg as IServiceBusMessagePropertiesProvider).TimeToLive.Value : message.TimeToLive;
                message.ContentType = (msg as IServiceBusMessagePropertiesProvider).ContentType ?? message.ContentType;
                message.Label = (msg as IServiceBusMessagePropertiesProvider).Label ?? message.Label;
                message.CorrelationId = (msg as IServiceBusMessagePropertiesProvider).CorrelationId ?? message.CorrelationId;
                message.ReplyToSessionId = (msg as IServiceBusMessagePropertiesProvider).ReplyToSessionId ?? message.ReplyToSessionId;
                message.SessionId = (msg as IServiceBusMessagePropertiesProvider).SessionId ?? message.SessionId;
                message.MessageId = (msg as IServiceBusMessagePropertiesProvider).MessageId ?? message.MessageId;
                message.PartitionKey = (msg as IServiceBusMessagePropertiesProvider).PartitionKey ?? message.PartitionKey;
                if (null != (msg as IServiceBusMessagePropertiesProvider).UserProperties)
                {
                    foreach (var t in (msg as IServiceBusMessagePropertiesProvider).UserProperties)
                        message.UserProperties.Add(t);
                }
            }

            if ((message is IActorMessageIdentifier) && String.IsNullOrWhiteSpace(message.MessageId))
            {
                message.MessageId = (msg as IActorMessageIdentifier).Id;
            }

            return message;
        }

        internal static Message SerializeAndSend(IActorMessage msg, ServiceBusQueue queue)
        {
            Message message = BuildMessage(msg);

            queue.SendAsync(message).Wait();
            return message;
        }

        public IActorMessage GetMessage()
        {
            IActorMessage msg = null;
            if (!msgQueue.TryDequeue(out msg))
                return null;

            return msg;
        }
        
        public void Kill()
        {
            var waitTask = queue.CloseAsync();

            foreach (var switchBoard in this.Switchboards)
                switchBoard.Disable();

            waitTask.Wait();
        }
        
        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
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
