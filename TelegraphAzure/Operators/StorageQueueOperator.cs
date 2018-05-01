﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Telegraphy.Azure.Exceptions;
using System.Collections.Concurrent;

namespace Telegraphy.Azure
{
    public abstract class StorageQueueOperator : IOperator
    {
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        CloudQueue queue = null;
        //MessageSerializationActor serializer = new MessageSerializationActor();
        //MessageDeserializationActor deserializer = new MessageDeserializationActor();
        TimeSpan? retrieveVisibilityTimeout = null;
        QueueRequestOptions retrievalRequestOptions = null;
        OperationContext retrievalOperationContext = null;
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        protected StorageQueueOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist, bool recieve, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, OperationContext retrievalOperationContext = null)
            : this (switchBoard, GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist), recieve, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }

        protected StorageQueueOperator(ILocalSwitchboard switchBoard, CloudQueue queue, bool recieve, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, OperationContext retrievalOperationContext = null)
        {
            this.recieveMessagesOnly = recieve;
            this.Switchboard = switchBoard;
            this.ID = 0;
            this.queue = queue;
            this.retrieveVisibilityTimeout = retrieveVisibilityTimeout;
            this.retrievalRequestOptions = retrievalRequestOptions;
            this.retrievalOperationContext = retrievalOperationContext;
            if (null != switchBoard)
                switchBoard.Operator = this;

            if (null == switchBoard && recieve)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }

        private static CloudQueue GetQueueFrom(string storageConnectionString,string queueName, bool createQueueIfItDoesNotExist)
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
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage(msg));
                byte[] msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
                //IActorMessage msg = t.Result as IActorMessage;
                //byte[] msgBytes = (serializer.Ask(new SerializeMessage(msg)).Result.ProcessingResult as byte[]);

                // Add the message to the azure queue
                CloudQueueMessage cloudMessage = new CloudQueueMessage(msgBytes);
                if (!(msg is IStorageQueuePropertiesProvider))
                {
                    this.queue.AddMessage(cloudMessage);
                }
                else
                {
                    IStorageQueuePropertiesProvider props = (msg as IStorageQueuePropertiesProvider);
                    this.queue.AddMessage(cloudMessage, props.TimeToLive, props.InitialVisibilityDelay, props.Options, props.OperationContext);
                }

                if (null != msg.Status)
                    msg.Status?.SetResult(new QueuedCloudMessage(cloudMessage));
            }
            catch(Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
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
                byte[] msgBytes = next.AsBytes;
                var t = Telegraph.Instance.Ask(new DeSerializeMessage(msgBytes));
                IActorMessage msg = t.Result as IActorMessage;

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
        
        public bool WaitTillEmpty(TimeSpan timeout)
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
