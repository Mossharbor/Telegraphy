using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using System.Transactions;
using Telegraphy.Net;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Messaging;

namespace Telegraphy.Msmq
{
    using System.Diagnostics;
    using Telegraphy.Msmq.Exceptions;
    using Telegraphy.Net.Exceptions;

    public class MsmqBaseOperator<MsgType> : IOperator where MsgType : class
    {
        public static ulong TotalSendsCalled = 0;
        protected const uint DefaultConncurrency = 3;
        protected const LocalConcurrencyType DefaultType = LocalConcurrencyType.DedicatedThreadCount;
        MessageQueue queue = null;
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        PerformanceCounter queueCounter = null;

        public long ID { get; set; }
        public ulong Count
        {
            get
            {
                try
                {
                    if (null == queueCounter)
                        return 0;

                    return (ulong)queueCounter.NextValue();
                }catch(InvalidOperationException)
                {
                    return 0;
                }
            }
        }

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }
        public MessageQueue Queue { get { return queue; }  set { queue = value; } }

        internal MsmqBaseOperator(ILocalSwitchboard switchboard, string machineName, string queueName) :
            this(machineName, queueName, QueueAccessMode.Receive)
        {
            this.Switchboards.Add(switchboard);
            switchboard.Operator = this;
            this.recieveMessagesOnly = true;
        }

        internal MsmqBaseOperator(string machineName, string queueName) :
            this(machineName, queueName, QueueAccessMode.Send)
        {
        }

        private MsmqBaseOperator(string machineName, string queueName, QueueAccessMode accessMode, EncryptionRequired encryptionRequired = EncryptionRequired.None)
        {
            string msmqName = MsmqHelper.CreateMsmqQueueName("", queueName, "", machineName);
            EnsureQueueExists(msmqName);
            var queueCounter = new PerformanceCounter(
               "MSMQ Queue",
               "Messages in Queue",
               msmqName,
               machineName);
            this.Queue = new MessageQueue(msmqName, accessMode);
            this.Queue.EncryptionRequired = encryptionRequired;
            Queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(MsgType) });
            if (!MessageQueue.Exists(this.Queue.Path))
                MessageQueue.Create(this.Queue.Path);

        }
        
        // Creates the queue if it does not already exist.
        public static void EnsureQueueExists(string path)
        {
            if (!MessageQueue.Exists(path))
            {
                MessageQueue.Create(path);
            }
        }
        
        private void SerializeAndSend(IActorMessage msg, MessageQueue msmqQueue, MsgType message)
        {
            ++TotalSendsCalled;
            var msmqMessage = new System.Messaging.Message(message);
            if (msg is IActorMessageIdentifier)
                msmqMessage.Label = (msg as IActorMessageIdentifier).Id;
            else if (message is IActorMessageIdentifier)
                msmqMessage.Label = (message as IActorMessageIdentifier).Id;

            //if (Transaction.Current == null)
            //{
            //    msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Single);
            //}
            //else
            //{
            msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Automatic);
            //}
        }

        #region IOperator
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
            
            // Serialize the message first
            try
            {
                if (msg is MsgType)
                    SerializeAndSend(msg, Queue, (MsgType)msg);
                else if ((msg as IActorMessage).Message is MsgType)
                    SerializeAndSend(msg, Queue, (MsgType)(msg as IActorMessage).Message);
                else
                    throw new DontKnowHowToSerializeTypeException(msg.GetType().ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetType()+":"+ex.Message + ex.StackTrace);
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
            }
        }

        public IActorMessage GetMessage()
        {
            try
            {
                System.Messaging.Message systemMessage = this.Queue.Receive(new TimeSpan(0, 0, 1), MessageQueueTransactionType.Single);
                systemMessage.Formatter = Queue.Formatter;
                object msg = systemMessage.Body;
                if (!(msg is IActorMessage))
                    return new SimpleMessage<MsgType>(msg as MsgType);
                return (msg as IActorMessage);
            }
            catch (System.Messaging.MessageQueueException)
            {
                return null;
            }
        }
        
        public void Kill()
        {
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
            while (0 != this.Count)
            {
                System.Threading.Thread.Sleep(1000);

                if ((DateTime.Now - start) > timeout)
                    return false;
            }

            return true;
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
