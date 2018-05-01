using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Telegraphy.Azure.Exceptions;
    using Telegraphy.Net;

    public class ServiceBusOperator : IOperator
    {
        public long ID { get; set; }

        public ulong Count { get; private set; }

        public ILocalSwitchboard Switchboard { get; set; }

        private MessageSender ServiceBusMsgSender = null;
        private MessageReceiver ServiceBusMsgReciever = null;
        
        protected ServiceBusOperator(MessageSender serviceBusMsgSender)
        {
            this.ServiceBusMsgSender = serviceBusMsgSender;
            this.ID = 0;
        }

        protected ServiceBusOperator(ILocalSwitchboard switchboard, MessageReceiver serviceBusMsgReciever)
        {
            this.Switchboard = switchboard;
            this.ServiceBusMsgReciever = serviceBusMsgReciever;
            this.ID = 0;
            if (null != Switchboard)
                Switchboard.Operator = this;

            if (null == switchboard)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }

        public void AddMessage(IActorMessage msg)
        {
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

            this.ServiceBusMsgSender.SendAsync(message);
        }

        public IActorMessage GetMessage()
        {
            System.Diagnostics.Debug.Assert(null != ServiceBusMsgReciever);
            throw new NotImplementedException();
        }

        public bool IsAlive()
        {
            throw new NotImplementedException();
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            throw new NotImplementedException();
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
            throw new NotImplementedException();
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            throw new NotImplementedException();
        }

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            throw new NotImplementedException();
        }
    }
}
