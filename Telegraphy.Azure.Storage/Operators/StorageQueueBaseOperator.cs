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
using Telegraphy.Azure.Exceptions;

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
            ILocalSwitchboard switchBoard,
            CloudQueue queue,
            CloudQueue deadLetterQueue,
            bool recieve,
            int maxDequeueCount = DefaultDequeueMaxCount,
            TimeSpan? retrieveVisibilityTimeout = null,
            QueueRequestOptions retrievalRequestOptions = null,
            Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
        {
            this.recieveMessagesOnly = recieve;
            this.Switchboard = switchBoard;
            this.ID = 0;
            this.queue = queue;
            this.deadLetterQueue = deadLetterQueue;
            this.retrieveVisibilityTimeout = retrieveVisibilityTimeout;
            this.retrievalRequestOptions = retrievalRequestOptions;
            this.retrievalOperationContext = retrievalOperationContext;
            if (null != switchBoard)
                switchBoard.Operator = this;

            if (null == switchBoard && recieve)
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

        public bool IsAlive()
        {
            // check and see if the azure queue exists.
            if (!this.queue.Exists())
                return false;
            return this.Switchboard.IsDisabled();
        }

        public void Kill()
        {
            this.Switchboard.Disable();
        }

        public ulong Count
        {
            get
            {
                return (ulong)this.queue.ApproximateMessageCount;
            }
        }

        public ILocalSwitchboard Switchboard { get; set; }

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
                this.Switchboard.Disable();
                return;
            }

            if (recieveMessagesOnly)
                throw new Telegraphy.Net.Exceptions.OperatorCannotSendMessagesException();

            // TODO allow the serializers to be passed in as IActors
            // Serialize the message first
            try
            {

                if (typeof(MsgType) == typeof(byte[]))
                    SerializeAndSend(msg, queue, (byte[])(msg as IActorMessage).Message);
                else if (typeof(MsgType) == typeof(string))
                    SerializeAndSend(msg, queue, (string)(msg as IActorMessage).Message);
                else if (msg.Message is IActorMessage)
                    SerializeAndSend((IActorMessage)msg.Message, queue);
                else
                    SerializeAndSend((IActorMessage)msg, queue);
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

        internal static void SerializeAndSend(IActorMessage msg, CloudQueue queue, byte[] msgBytes = null)
        {
            if (null == msgBytes)
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeIActorMessage(msg)); //TODO timeout the wait here!!
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }

            // Add the message to the azure queue
            CloudQueueMessage cloudMessage = new CloudQueueMessage((string)null);
            cloudMessage.SetMessageContent(msgBytes);

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
                    var t = Telegraph.Instance.Ask(new DeserializeIActorMessage(msgBytes));
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

        public void Register<T>(Action<T> action) where T : class
        {
            if (null == this.Switchboard && !recieveMessagesOnly)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
            where T : class
            where K : IActor
        {
            if (null == this.Switchboard && !recieveMessagesOnly)
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
#endregion
    }
}
