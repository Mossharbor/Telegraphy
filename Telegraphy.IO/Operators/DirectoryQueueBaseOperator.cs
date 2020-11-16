using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.IO.Exceptions;
using System.Collections.Concurrent;
using Telegraphy.Net.Exceptions;


namespace Telegraphy.IO
{

    public abstract class DirectoryQueueBaseOperator<MsgType> : IOperator where MsgType:class
    {
        internal const int DefaultDequeueMaxCount = 1;
        internal const int DefaultConcurrency = 3;

        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        DirectoryQueue queue = null;
        DirectoryQueue deadLetterQueue = null;
        TimeSpan? retrieveVisibilityTimeout = null;
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        private int maxDequeueCount = 1;

        protected DirectoryQueueBaseOperator(
            ILocalSwitchboard switchBoard,
            string queueDirectoryRoot, 
            string queueName, 
            bool createQueueIfItDoesNotExist,
            bool recieveMessagesOnly, 
            int maxDequeueCount = DefaultDequeueMaxCount,
            TimeSpan? retrieveVisibilityTimeout = null)
            : this (switchBoard,
                  GetQueueFrom(queueDirectoryRoot, queueName, createQueueIfItDoesNotExist),
                  GetDeadLetterQueueFrom(queueDirectoryRoot, queueName),
                  recieveMessagesOnly, 
                  maxDequeueCount,
                  retrieveVisibilityTimeout)
        {
        }

        protected DirectoryQueueBaseOperator(
            ILocalSwitchboard switchboard,
            DirectoryQueue queue,
            DirectoryQueue deadLetterQueue,
            bool recieveMessagesOnly,
            int maxDequeueCount = DefaultDequeueMaxCount,
            TimeSpan? retrieveVisibilityTimeout = null)
        {
            this.recieveMessagesOnly = recieveMessagesOnly;
            if (null != switchboard)
            {
                this.Switchboards.Add(switchboard);
                switchboard.Operator = this;
            }
            this.ID = 0;
            this.queue = queue;
            this.deadLetterQueue = deadLetterQueue;
            this.retrieveVisibilityTimeout = retrieveVisibilityTimeout;

            if (null == switchboard && recieveMessagesOnly)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }
        
        internal static DirectoryQueue GetQueueFrom(string queueRootDirectory, string queueName, bool createQueueIfItDoesNotExist)
        {
            DirectoryQueue queue = new DirectoryQueue(queueRootDirectory, queueName);

            if (createQueueIfItDoesNotExist)
                queue.CreateIfNotExists();

            return queue;
        }

        internal static DirectoryQueue GetDeadLetterQueueFrom(string queueRootDirectory, string queueName)
        {
            using (DirectoryQueue queue = new DirectoryQueue(queueRootDirectory, queueName))
            {
                queue.CreateIfNotExists();
                return queue;
            }
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

        internal static void SerializeAndSend(IActorMessage msg, DirectoryQueue queue, string msgString)
        {
            DirectoryQueueMessage cloudMessage = new DirectoryQueueMessage();
            cloudMessage.SetMessageContent(msgString);
            AddMessageProperties(queue, cloudMessage, (msg is IStorageQueuePropertiesProvider) ? (msg as IStorageQueuePropertiesProvider) : null);
            
            if (null != msg.Status)
                msg.Status?.SetResult(new QueuedDirectoryMessage(cloudMessage));
        }

        internal static DirectoryQueueMessage BuildMessage(IActorMessage msg)
        {
            DirectoryQueueMessage cloudMessage = new DirectoryQueueMessage();
            
            byte[] msgBytes = TempSerialization.GetBytes<MsgType>(msg);
            cloudMessage.SetMessageContent(msgBytes);
            return cloudMessage;
        }

        internal static void SerializeAndSend(IActorMessage msg, DirectoryQueue queue)
        {
            // Add the message to the azure queue
            DirectoryQueueMessage cloudMessage = BuildMessage(msg);

            AddMessageProperties(queue, cloudMessage, (msg is IStorageQueuePropertiesProvider) ? (msg as IStorageQueuePropertiesProvider): null);

            if (null != msg.Status)
                msg.Status?.SetResult(new QueuedDirectoryMessage(cloudMessage));
        }

        internal static void AddMessageProperties(DirectoryQueue queue, DirectoryQueueMessage cloudMessage, IStorageQueuePropertiesProvider props)
        {
            if (null == props || (null == props.TimeToLive && null == props.InitialVisibilityDelay))
            {
                queue.AddMessage(cloudMessage);
            }
            else
            {
                queue.AddMessage(cloudMessage, props.TimeToLive, props.InitialVisibilityDelay);
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
                DirectoryQueueMessage next = this.queue.GetMessage(this.retrieveVisibilityTimeout);

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
                System.Threading.Thread.Sleep(100);
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
