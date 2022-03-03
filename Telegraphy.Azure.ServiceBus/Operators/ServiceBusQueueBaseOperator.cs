using global::Azure.Messaging.ServiceBus;
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
        private ServiceBusReceiver reciever;
        private ServiceBusSender sender;
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
            {
                if (string.IsNullOrEmpty(queue.Subscription))
                    reciever = queue.CreateReceiver(queue.QueueName, new ServiceBusReceiverOptions() { ReceiveMode = queue.ReceiveMode, SubQueue = queue.SubQueue });
                else
                    reciever = queue.CreateReceiver(queue.TopicName, queue.Subscription, new ServiceBusReceiverOptions() { ReceiveMode = queue.ReceiveMode, SubQueue = queue.SubQueue });
            }
            else
            {
                if (string.IsNullOrEmpty(queue.TopicName))
                    sender = queue.CreateSender(queue.QueueName);
                else
                    sender = queue.CreateSender(queue.TopicName);
            }

            //var processor = queue.CreateSessionProcessor(queue.QueueName, new ServiceBusSessionProcessorOptions() { ReceiveMode = queue.ReceiveMode });
            //processor
        }


        private Task RecieveMessages(ServiceBusReceivedMessage sbMessage, CancellationToken token)
        {
            if (sbMessage.DeliveryCount >= maxDequeueCount)
            {
                return reciever.DeadLetterMessageAsync(sbMessage, sbMessage.LockToken);
            }

            IActorMessage msg = null;
            if (typeof(MsgType) == typeof(byte[]))
                msg = sbMessage.Body.ToArray().ToActorMessage(); 
            else if (typeof(MsgType) == typeof(string))
                msg = Encoding.UTF8.GetString(sbMessage.Body.ToArray()).ToActorMessage();
            else
            {
                byte[] msgBytes = sbMessage.Body.ToArray();
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
                    reciever.AbandonMessageAsync(sbMessage, null, token).Wait();
                else
                    reciever.CompleteMessageAsync(sbMessage, token).Wait();
            });

            return msg.Status.Task;
        }

        /*private Task HandleExceptions(ExceptionReceivedEventArgs eargs)
        {
            return Task.Factory.StartNew(() =>
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, eargs.Exception, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
            });
        }*/

        #region IOperator
        public long ID { get; set; }

        public ulong Count { get { return (ulong)queue.ApproximateMessageCount; } }

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }

        public void AddMessage(IActorMessage msg)
        {
            try
            {
                if (msg is ControlMessages.HangUp)
                {
                    if (null != hangUp)
                        return;

                    if (null != queue)
                    {
                        this.reciever?.CloseAsync().Wait();
                        this.sender?.CloseAsync().Wait();

                        if (null != msg.Status && !msg.Status.Task.IsCompleted)
                            msg.Status.SetResult(msg);
                        return;
                    }


                    this.reciever?.CloseAsync().Wait();
                    this.sender?.CloseAsync().Wait();
                    hangUp = (msg as ControlMessages.HangUp);
                    foreach (var switchboard in this.Switchboards)
                        switchboard.Disable();
                    return;
                }

                if (this.recievingOnly)
                    throw new OperatorCannotSendMessagesException();

                System.Diagnostics.Debug.Assert(null != sender);
                var message = SerializeAndSend(msg, this.sender);

                if (null != msg.Status)
                    msg.Status?.SetResult(new ServiceBusMessage(message));
            }
            catch (Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);

                if (null != msg.Status && !msg.Status.Task.IsCanceled)
                    msg.Status.TrySetException(ex);
            }
        }

        internal static global::Azure.Messaging.ServiceBus.ServiceBusMessage BuildMessage(IActorMessage msg)
        {
            byte[] msgBytes = TempSerialization.GetBytes<MsgType>(msg);

            var message = new global::Azure.Messaging.ServiceBus.ServiceBusMessage(msgBytes);

            if (msg is IServiceBusMessagePropertiesProvider)
            {
                message.ScheduledEnqueueTime = (msg as IServiceBusMessagePropertiesProvider).ScheduledEnqueueTimeUtc.HasValue ? (msg as IServiceBusMessagePropertiesProvider).ScheduledEnqueueTimeUtc.Value : message.ScheduledEnqueueTime;
                message.TimeToLive = (msg as IServiceBusMessagePropertiesProvider).TimeToLive.HasValue ? (msg as IServiceBusMessagePropertiesProvider).TimeToLive.Value : message.TimeToLive;
                message.ContentType = (msg as IServiceBusMessagePropertiesProvider).ContentType ?? message.ContentType;
                message.CorrelationId = (msg as IServiceBusMessagePropertiesProvider).CorrelationId ?? message.CorrelationId;
                message.ReplyToSessionId = (msg as IServiceBusMessagePropertiesProvider).ReplyToSessionId ?? message.ReplyToSessionId;
                message.SessionId = (msg as IServiceBusMessagePropertiesProvider).SessionId ?? message.SessionId;
                message.MessageId = (msg as IServiceBusMessagePropertiesProvider).MessageId ?? message.MessageId;
                message.PartitionKey = (msg as IServiceBusMessagePropertiesProvider).PartitionKey ?? message.PartitionKey;
                if (null != (msg as IServiceBusMessagePropertiesProvider).ApplicationProperties)
                {
                    foreach (var t in (msg as IServiceBusMessagePropertiesProvider).ApplicationProperties)
                        message.ApplicationProperties.Add(t);
                }
            }

            if ((message is IActorMessageIdentifier) && String.IsNullOrWhiteSpace(message.MessageId))
            {
                message.MessageId = (msg as IActorMessageIdentifier).Id;
            }

            return message;
        }

        internal static global::Azure.Messaging.ServiceBus.ServiceBusMessage SerializeAndSend(IActorMessage msg, ServiceBusSender sender)
        {
            global::Azure.Messaging.ServiceBus.ServiceBusMessage message = BuildMessage(msg);

            sender.SendMessageAsync(message).Wait();
            return message;
        }

        public IActorMessage GetMessage()
        {
            try
            {
                IActorMessage msg = null;
                if (!msgQueue.TryDequeue(out msg))
                    return null;

                return msg;
            }
            catch (Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
                return null;
            }
        }
        
        public void Kill()
        {
            this.reciever?.CloseAsync().Wait();
            this.sender?.CloseAsync().Wait();

            foreach (var switchBoard in this.Switchboards)
                switchBoard.Disable();
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
