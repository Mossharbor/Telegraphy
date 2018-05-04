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
using System.ServiceModel;

namespace Telegraphy.Azure
{
    public enum MessageSource { ByteArrayMessage =0, StringMessage = 1, EntireIActor=2 }

    public abstract class StorageQueueBaseOperator : IOperator
    {
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        CloudQueue queue = null;
        TimeSpan? retrieveVisibilityTimeout = null;
        QueueRequestOptions retrievalRequestOptions = null;
        Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null;
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        MessageSource messageSource = MessageSource.EntireIActor;
        private System.ServiceModel.OperationContext retrievalOperationContext1;

        protected StorageQueueBaseOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist, bool recieve, MessageSource messageSource = MessageSource.EntireIActor, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : this (switchBoard, GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist), recieve, messageSource, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }

        protected StorageQueueBaseOperator(ILocalSwitchboard switchBoard, CloudQueue queue, bool recieve, MessageSource messageSource = MessageSource.EntireIActor, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
        {
            this.recieveMessagesOnly = recieve;
            this.Switchboard = switchBoard;
            this.ID = 0;
            this.queue = queue;
            this.retrieveVisibilityTimeout = retrieveVisibilityTimeout;
            this.retrievalRequestOptions = retrievalRequestOptions;
            this.retrievalOperationContext = retrievalOperationContext;
            this.messageSource = messageSource;
            if (null != switchBoard)
                switchBoard.Operator = this;

            if (null == switchBoard && recieve)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }
        
        internal static CloudQueue GetQueueFrom(string storageConnectionString,string queueName, bool createQueueIfItDoesNotExist)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName.ToLower());

            if (createQueueIfItDoesNotExist)
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
                throw new OperatorCannotSendMessagesException();

            // TODO allow the serializers to be passed in as IActors
            // Serialize the message first
            try
            {
                switch(messageSource)
                {
                    case MessageSource.StringMessage:
                        if ((msg as IActorMessage).Message.GetType().Name.Equals("String"))
                            SerializeAndSend(msg, queue, (string)(msg as IActorMessage).Message);
                        else
                            throw new OperatorIsNotConfiguredToSerializeThisTypeOfMessageException("String");
                        break;
                    case MessageSource.ByteArrayMessage:
                        if ((msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                            SerializeAndSend(msg, queue, (byte[])(msg as IActorMessage).Message);
                        else
                            throw new OperatorIsNotConfiguredToSerializeThisTypeOfMessageException("Byte[]");
                        break;
                    case MessageSource.EntireIActor:
                        SerializeAndSend(msg, queue);
                        break;
                    default:
                        throw new OperatorIsNotConfiguredToSerializeThisTypeOfMessageException(messageSource.ToString());
                }
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
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage(msg));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }

            // Add the message to the azure queue
            CloudQueueMessage cloudMessage = new CloudQueueMessage(msgBytes);
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

                IActorMessage msg = null;
                switch (messageSource)
                {
                    case MessageSource.EntireIActor:
                        {
                            byte[] msgBytes = next.AsBytes;
                            var t = Telegraph.Instance.Ask(new DeSerializeMessage(msgBytes));
                            msg = t.Result as IActorMessage;
                        }
                        break;
                    case MessageSource.StringMessage:
                        msg = next.AsString.ToActorMessage();
                        break;
                    case MessageSource.ByteArrayMessage:
                        msg = next.AsBytes.ToActorMessage();
                        break;
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
            //where T : IActorMessage
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
