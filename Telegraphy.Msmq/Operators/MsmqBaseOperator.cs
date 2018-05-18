﻿using System;
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

    public class MsmqBaseOperator : IOperator
    {
        MessageQueue queue = null;
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        PerformanceCounter queueCounter = null;
        MessageSource messageSource = MessageSource.EntireIActor;

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

        public ILocalSwitchboard Switchboard { get; set; }
        
        public MsmqBaseOperator(string machineName, string queueName, string[] targetTypeNames, MessageSource messageSource) :
            this(machineName, queueName, QueueAccessMode.Receive, messageSource)
        {
            this.recieveMessagesOnly = true;
            ((XmlMessageFormatter)queue.Formatter).TargetTypeNames = targetTypeNames;
        }

        public MsmqBaseOperator(string machineName, string queueName, MessageSource messageSource) :
            this(machineName, queueName, QueueAccessMode.Send, messageSource)
        {
        }

        private MsmqBaseOperator(string machineName, string queueName, QueueAccessMode accessMode, MessageSource messageSource)
        {
            this.messageSource = messageSource;
            string msmqName = MsmqHelper.CreateMsmqQueueName("", queueName, "", machineName);
            EnsureQueueExists(msmqName);
            var queueCounter = new PerformanceCounter(
               "MSMQ Queue",
               "Messages in Queue",
               msmqName,
               machineName);
            this.queue = new MessageQueue(msmqName, accessMode);

        }
        
        // Creates the queue if it does not already exist.
        public static void EnsureQueueExists(string path)
        {
            if (!MessageQueue.Exists(path))
            {
                MessageQueue.Create(path);
            }
        }

        private void SerializeAndSend(IActorMessage msg, MessageQueue msmqQueue)
        {
            var msmqMessage = new System.Messaging.Message((msg as IActorMessage).Message);
            if (Transaction.Current == null)
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Single);
            }
            else
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Automatic);
            }
        }

        private void SerializeAndSend(IActorMessage msg, MessageQueue msmqQueue, byte[] message)
        {
            var msmqMessage = new System.Messaging.Message(message);
            if (Transaction.Current == null)
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Single);
            }
            else
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Automatic);
            }
        }

        private void SerializeAndSend(IActorMessage msg, MessageQueue msmqQueue, string message)
        {
            var msmqMessage = new System.Messaging.Message(message);
            if (Transaction.Current == null)
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Single);
            }
            else
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Automatic);
            }
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
                this.Switchboard.Disable();
                return;
            }

            if (recieveMessagesOnly)
                throw new Telegraphy.Net.Exceptions.OperatorCannotSendMessagesException();
            
            // Serialize the message first
            try
            {
                switch (messageSource)
                {
                    case MessageSource.StringMessage:
                        if ((msg as IActorMessage).Message.GetType().Name.Equals("String"))
                            SerializeAndSend(msg, queue, (string)(msg as IActorMessage).Message);
                        else
                            throw new NotConfiguredToSerializeThisTypeOfMessageException("String");
                        break;
                    case MessageSource.ByteArrayMessage:
                        if ((msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                            SerializeAndSend(msg, queue, (byte[])(msg as IActorMessage).Message);
                        else
                            throw new NotConfiguredToSerializeThisTypeOfMessageException("Byte[]");
                        break;
                    case MessageSource.EntireIActor:
                        SerializeAndSend(msg, queue);
                        break;
                    default:
                        throw new NotConfiguredToSerializeThisTypeOfMessageException(messageSource.ToString());
                }
            }
            catch (Exception ex)
            {
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
                System.Messaging.Message systemMessage = this.queue.Receive(new TimeSpan(0, 0, 1), MessageQueueTransactionType.Single);
                object msg = systemMessage.Body;
                return (IActorMessage)msg;
            }
            catch (System.Messaging.MessageQueueException)
            {
                return null;
            }
        }

        public bool IsAlive()
        {
            // check and see if the azure queue exists.
            if (!MessageQueue.Exists(queue.Path))
                return false;
            return this.Switchboard.IsDisabled();
        }

        public void Kill()
        {
            this.Switchboard.Disable();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            if (null == this.Switchboard && !recieveMessagesOnly)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(Expression<Func<K>> factory)
            where T : class
            where K : IActor
        {
            if (null == this.Switchboard && !recieveMessagesOnly)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException();

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
