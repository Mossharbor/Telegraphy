using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Azure.Storage;
using Azure.Storage.Queues;
using Telegraphy.Azure.Exceptions;
using System.Collections.Concurrent;
using Telegraphy.Net.Exceptions;
using Azure.Storage.Queues.Models;

namespace Telegraphy.Azure
{

    public abstract class StorageQueueBaseOperator<MsgType> : IOperator where MsgType:class
    {
        internal const int DefaultDequeueMaxCount = 1;
        internal const int DefaultConcurrency = 3;

        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        QueueClient queue = null;
        QueueClient deadLetterQueue = null;
        TimeSpan? retrieveVisibilityTimeout = null;
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        private int maxDequeueCount = 1;

        protected StorageQueueBaseOperator(
            ILocalSwitchboard switchBoard,
            string storageConnectionString, 
            string queueName, 
            bool createQueueIfItDoesNotExist,
            bool recieve, 
            int maxDequeueCount = DefaultDequeueMaxCount,
            TimeSpan? retrieveVisibilityTimeout = null)
            : this (switchBoard,
                  GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist),
                  GetDeadLetterQueueFrom(storageConnectionString, queueName),
                  recieve, 
                  maxDequeueCount,
                  retrieveVisibilityTimeout)
        {
        }

        protected StorageQueueBaseOperator(
            ILocalSwitchboard switchboard,
            QueueClient queue,
            QueueClient deadLetterQueue,
            bool recieve,
            int maxDequeueCount = DefaultDequeueMaxCount,
            TimeSpan? retrieveVisibilityTimeout = null)
        {
            this.recieveMessagesOnly = recieve;
            if (null != switchboard)
            {
                this.Switchboards.Add(switchboard);
                switchboard.Operator = this;
            }
            this.ID = 0;
            this.queue = queue;
            this.deadLetterQueue = deadLetterQueue;
            this.retrieveVisibilityTimeout = retrieveVisibilityTimeout;

            if (null == switchboard && recieve)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }
        
        internal static QueueClient GetQueueFrom(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist)
        {
            QueueClient queue = new QueueClient(storageConnectionString, queueName);

            if (createQueueIfItDoesNotExist)
                queue.CreateIfNotExists();

            return queue;
        }

        internal static QueueClient GetDeadLetterQueueFrom(string storageConnectionString, string queueName)
        {
            QueueClient queue = new QueueClient(storageConnectionString, queueName+"-deadletter".ToLower());

            queue.CreateIfNotExists();

            return queue;
        }

        #region IActor

        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            AddMessage(msg);
            return true;
        }
        #endregion

        #region IOperator

        public long ID { get; set; }
        
        public void Kill()
        {
            foreach (var switchBoard in this.Switchboards)
                switchBoard.Disable();
        }

        public ulong Count
        {
            get
            {
                return (ulong)this.queue.GetProperties().Value.ApproximateMessagesCount;
            }
        }

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }

        public void AddMessage(IActorMessage msg)
        {
            if (msg is ControlMessages.HangUp)
            {
                if (!recieveMessagesOnly)
                {
                    if (null != msg.Status && !msg.Status.Task.IsCompleted)
                        msg.Status.SetResult(msg);
                    return;
                }
                
                hangUp = (msg as ControlMessages.HangUp);
                Kill();
                return;
            }

            if (recieveMessagesOnly)
                throw new Telegraphy.Net.OperatorCannotSendMessagesException(msg.GetType().ToString());

            // TODO allow the serializers to be passed in as IActors
            // Serialize the message first
            try
            {
                SerializeAndSend(msg, queue);

                if (null != msg.Status && !msg.Status.Task.IsCanceled)
                    msg.Status.TrySetResult(msg);
            }
            catch(Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
            }
        }

        internal static void SerializeAndSend(IActorMessage msg, QueueClient queue, string msgString)
        {
            // string encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes(msgString)); // azure portal assumes base 64
            var reciept = queue.SendMessage(msgString, visibilityTimeout: (msg as IStorageQueuePropertiesProvider)?.InitialVisibilityDelay, timeToLive: (msg as IStorageQueuePropertiesProvider)?.TimeToLive);

            if (null != msg.Status)
                msg.Status?.SetResult(new QueuedCloudMessage(msgString, reciept.Value));
        }

        internal static BinaryData BuildMessage(IActorMessage msg)
        {
            byte[] msgBytes = TempSerialization.GetBytes<MsgType>(msg);
            return BinaryData.FromString(Convert.ToBase64String(msgBytes));
        }

        internal static void SerializeAndSend(IActorMessage msg, QueueClient queue)
        {
            // Add the message to the azure queue
            BinaryData cloudMessage = BuildMessage(msg);

            var reciept = queue.SendMessage(cloudMessage, visibilityTimeout: (msg as IStorageQueuePropertiesProvider)?.InitialVisibilityDelay, timeToLive: (msg as IStorageQueuePropertiesProvider)?.TimeToLive);

            if (null != msg.Status)
                msg.Status?.SetResult(new QueuedCloudMessage(cloudMessage, reciept.Value));
        }

        public IActorMessage GetMessage()
        {
            if (!recieveMessagesOnly)
                throw new OperatorCannotRecieveMessagesException();

            if (null != hangUp)
                return hangUp;

            try
            {
                // Get the next message off of the azure queue
                QueueMessage next = this.queue.ReceiveMessage(this.retrieveVisibilityTimeout);

                if (null == next)
                    return null;
                
                if (next.DequeueCount > maxDequeueCount)
                {
                    if (null != this.deadLetterQueue)
                         deadLetterQueue.SendMessage(next.Body);

                    var response = queue.DeleteMessage(next.MessageId, next.PopReceipt);
                    
                    return null;
                }

                IActorMessage msg = null;
                if (typeof(MsgType) == typeof(string))
                {
                    string body = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(next.Body.ToArray()))); // azure portal assumes base 64
                    msg = body.ToActorMessage();
                }
                else if (typeof(MsgType) == typeof(byte[]))
                {
                    var base64String = next.Body.ToString();
                    var messageBytes = Convert.FromBase64String(base64String);
                    msg = messageBytes.ToActorMessage();
                }
                else
                {
                    var base64String = next.Body.ToString();
                    var messageBytes = Convert.FromBase64String(base64String);
                    var t = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(messageBytes));
                    msg = t.Result as IActorMessage;
                }

                if (null == msg.Status)
                    msg.Status = new TaskCompletionSource<IActorMessage>();

                msg.Status.Task.ContinueWith(p => queue.DeleteMessage(next.MessageId, next.PopReceipt));
                return msg;
            }
            catch(Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
                return null;
            }
        }
        
        public virtual bool WaitTillEmpty(TimeSpan timeout)
        {
            // wait till the azure queue is currently empty
            DateTime start = DateTime.Now;
            
            while (0 != this.queue.GetProperties().Value.ApproximateMessagesCount && (DateTime.Now - start) < timeout)
            {
                System.Threading.Thread.Sleep(1000);
            }

            return ((DateTime.Now - start) <= timeout);
        }
        
        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }
#endregion
    }
}
