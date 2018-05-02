﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using System.Collections.Concurrent;
    using System.Threading;
    using Telegraphy.Azure.Exceptions;
    using Telegraphy.Net;

    public class ServiceBusTopicOperator : IOperator
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private int maxDequeueCount = 1;
        private MessageSender ServiceBusMsgSender = null;
        private MessageReceiver ServiceBusMsgReciever = null;
        ControlMessages.HangUp hangUp = null;
        ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();
        
        protected ServiceBusTopicOperator(MessageSender serviceBusMsgSender)
        {
            this.ServiceBusMsgSender = serviceBusMsgSender;
            this.ID = 0;
        }

        protected ServiceBusTopicOperator(ILocalSwitchboard switchboard, MessageReceiver serviceBusMsgReciever, int maxDequeueCount)
        {
            this.maxDequeueCount = maxDequeueCount;
            this.Switchboard = switchboard;
            this.ServiceBusMsgReciever = serviceBusMsgReciever;
            this.ID = 0;
            if (null != Switchboard)
                Switchboard.Operator = this;

            if (null == switchboard)
                throw new SwitchBoardNeededWhenRecievingMessagesException();

            MessageHandlerOptions options = new MessageHandlerOptions(HandleExceptions);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = (int)switchboard.Concurrency;
            
            this.ServiceBusMsgReciever.RegisterMessageHandler(ListenForMessages, options);
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

        private Task ListenForMessages(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken token)
        {
            if (sbMessage.SystemProperties.DeliveryCount > maxDequeueCount)
            {
                return this.ServiceBusMsgReciever.AbandonAsync(sbMessage.SystemProperties.LockToken);
            }

            byte[] msgBytes = sbMessage.Body;
            var t = Telegraph.Instance.Ask(new DeSerializeMessage(msgBytes));
            IActorMessage msg = t.Result as IActorMessage;

            msgQueue.Enqueue(msg);

            if (null == msg.Status)
                msg.Status = new TaskCompletionSource<IActorMessage>();
            
            msg.Status.Task.ContinueWith(p => this.ServiceBusMsgReciever.CompleteAsync(sbMessage.SystemProperties.LockToken));

            return msg.Status.Task;
        }

        #region IOperator

        public long ID { get; set; }

        public ulong Count { get { return (ulong)msgQueue.Count; } }

        public ILocalSwitchboard Switchboard { get; set; }

        public void AddMessage(IActorMessage msg)
        {
            if (msg is ControlMessages.HangUp)
            {
                if (null != hangUp)
                    return;

                if (null != ServiceBusMsgSender)
                {
                    ServiceBusMsgReciever.CloseAsync().Wait();
                    if (null != msg.Status && !msg.Status.Task.IsCompleted)
                        msg.Status.SetResult(msg);
                    return;
                }

                ServiceBusMsgReciever.CloseAsync().Wait();
                hangUp = (msg as ControlMessages.HangUp);
                this.Switchboard.Disable();
                return;
            }

            System.Diagnostics.Debug.Assert(null != ServiceBusMsgSender);
            byte[] serializedMessage = null; //TODO

            var message = new Message(serializedMessage);

            if (msg is IServiceBusPropertiesProvider)
            {
                message.ScheduledEnqueueTimeUtc = (msg as IServiceBusPropertiesProvider).ScheduledEnqueueTimeUtc.HasValue ? (msg as IServiceBusPropertiesProvider).ScheduledEnqueueTimeUtc.Value : message.ScheduledEnqueueTimeUtc;
                message.TimeToLive = (msg as IServiceBusPropertiesProvider).TimeToLive.HasValue ? (msg as IServiceBusPropertiesProvider).TimeToLive.Value : message.TimeToLive;
                message.ContentType = (msg as IServiceBusPropertiesProvider).ContentType ?? message.ContentType;
                message.Label = (msg as IServiceBusPropertiesProvider).Label ?? message.Label;
                message.CorrelationId = (msg as IServiceBusPropertiesProvider).CorrelationId ?? message.CorrelationId;
                message.ReplyToSessionId = (msg as IServiceBusPropertiesProvider).ReplyToSessionId ?? message.ReplyToSessionId;
                message.SessionId = (msg as IServiceBusPropertiesProvider).SessionId ?? message.SessionId;
                message.MessageId = (msg as IServiceBusPropertiesProvider).MessageId ?? message.MessageId;
            }

            if ((message is IActorMessageIdentifier) && String.IsNullOrWhiteSpace(message.MessageId))
            {
                message.MessageId = (msg as IActorMessageIdentifier).Id;
            }

            this.ServiceBusMsgSender.SendAsync(message).Wait();

            if (null != msg.Status)
                msg.Status?.SetResult(new ServiceBusMessage(message));
        }
        
        public IActorMessage GetMessage()
        {
            System.Diagnostics.Debug.Assert(null != ServiceBusMsgReciever);

            IActorMessage msg = null;
            if (!msgQueue.TryDequeue(out msg))
                return null;
            
            return msg;
        }

        public bool IsAlive()
        {
            if (null == ServiceBusMsgReciever ? this.ServiceBusMsgSender.IsClosedOrClosing : this.ServiceBusMsgReciever.IsClosedOrClosing)
                return false;
            
            return this.Switchboard.IsDisabled();
        }

        public void Kill()
        {
            if (null != this.ServiceBusMsgSender)
            {
                this.ServiceBusMsgSender.CloseAsync().Wait();
            }
            else
            {
                this.ServiceBusMsgReciever.CloseAsync().Wait();
            }

            this.Switchboard.Disable();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            if (null == this.Switchboard && null != ServiceBusMsgReciever)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
            if (null == this.Switchboard && null != ServiceBusMsgReciever)
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

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while (0 != this.msgQueue.Count && (DateTime.Now - start) < timeout)
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
