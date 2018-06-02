using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Telegraphy.Azure.Exceptions;
using System.Collections.Concurrent;
using Telegraphy.Net.Exceptions;
using Mossharbor.AzureWorkArounds.Storage;

namespace Telegraphy.Azure
{

    public abstract class StorageQueueBaseOperator<MsgType> : IOperator where MsgType:class
    {
        internal const int DefaultDequeueMaxCount = 1;
        internal const int DefaultConcurrency = 3;

        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        CloudQueue queue = null;
        CloudQueue deadLetterQueue = null;
        TimeSpan? retrieveVisibilityTimeout = null;
        QueueRequestOptions retrievalRequestOptions = null;
        Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null;
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
            TimeSpan? retrieveVisibilityTimeout = null, 
            QueueRequestOptions retrievalRequestOptions = null,
            Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : this (switchBoard,
                  GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist),
                  GetDeadLetterQueueFrom(storageConnectionString, queueName),
                  recieve, 
                  maxDequeueCount,
                  retrieveVisibilityTimeout,
                  retrievalRequestOptions,
                  retrievalOperationContext)
        {
        }

        protected StorageQueueBaseOperator(
            ILocalSwitchboard switchboard,
            CloudQueue queue,
            CloudQueue deadLetterQueue,
            bool recieve,
            int maxDequeueCount = DefaultDequeueMaxCount,
            TimeSpan? retrieveVisibilityTimeout = null,
            QueueRequestOptions retrievalRequestOptions = null,
            Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
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
            this.retrievalRequestOptions = retrievalRequestOptions;
            this.retrievalOperationContext = retrievalOperationContext;

            if (null == switchboard && recieve)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }
        
        internal static CloudQueue GetQueueFrom(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName.ToLower());

            if (createQueueIfItDoesNotExist)
                queue.CreateIfNotExists();

            return queue;
        }

        internal static CloudQueue GetDeadLetterQueueFrom(string storageConnectionString, string queueName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName+"-deadletter".ToLower());

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
                return (ulong)this.queue.ApproximateMessageCount;
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
                throw new Telegraphy.Net.OperatorCannotSendMessagesException();

            // TODO allow the serializers to be passed in as IActors
            // Serialize the message first
            try
            {
                SerializeAndSend(msg, queue);
            }
            catch(Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
            }
        }

        internal static void SerializeAndSend(IActorMessage msg, CloudQueue queue, string msgString)
        {
            CloudQueueMessage cloudMessage = new CloudQueueMessage(msgString);
            AddMessageProperties(queue, cloudMessage, (msg is IStorageQueuePropertiesProvider) ? (msg as IStorageQueuePropertiesProvider) : null);
            
            if (null != msg.Status)
                msg.Status?.SetResult(new QueuedCloudMessage(cloudMessage));
        }

        internal static CloudQueueMessage BuildMessage(IActorMessage msg)
        {
            CloudQueueMessage cloudMessage = new CloudQueueMessage((string)null);
            
            byte[] msgBytes = TempSerialization.GetBytes<MsgType>(msg);
            cloudMessage.SetMessageContent(msgBytes);
            return cloudMessage;
        }

        internal static void SerializeAndSend(IActorMessage msg, CloudQueue queue)
        {
            // Add the message to the azure queue
            CloudQueueMessage cloudMessage = BuildMessage(msg);

            AddMessageProperties(queue, cloudMessage, (msg is IStorageQueuePropertiesProvider) ? (msg as IStorageQueuePropertiesProvider): null);

            if (null != msg.Status)
                msg.Status?.SetResult(new QueuedCloudMessage(cloudMessage));
        }

        internal static void AddMessageProperties(CloudQueue queue, CloudQueueMessage cloudMessage, IStorageQueuePropertiesProvider props)
        {
            if (null == props)
            {
                queue.AddMessage(cloudMessage);
            }
            else
            {
                queue.AddMessage(cloudMessage, props.TimeToLive, props.InitialVisibilityDelay, props.Options, props.OperationContext);
            }
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
                CloudQueueMessage next = this.queue.GetMessage(this.retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext);

                if (null == next)
                    return null;
                
                if (next.DequeueCount > maxDequeueCount)
                {
                    if (null != this.deadLetterQueue)
                        deadLetterQueue.AddMessage(next);
                    queue.DeleteMessage(next);
                    return null;
                }

                IActorMessage msg = null;
                if (typeof(MsgType) == typeof(string))
                    msg = next.AsString.ToActorMessage();
                else if (typeof(MsgType) == typeof(byte[]))
                     msg = next.AsBytes.ToActorMessage();
                else
                {
                    byte[] msgBytes = next.AsBytes;
                    var t = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(msgBytes));
                    msg = t.Result as IActorMessage;
                }

                if (null == msg.Status)
                    msg.Status = new TaskCompletionSource<IActorMessage>();

                msg.Status.Task.ContinueWith(p => queue.DeleteMessage(next));
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
            this.queue.FetchAttributes();
            while (0 != this.queue.ApproximateMessageCount && (DateTime.Now - start) < timeout)
            {
                System.Threading.Thread.Sleep(1000);
                this.queue.FetchAttributes();
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
