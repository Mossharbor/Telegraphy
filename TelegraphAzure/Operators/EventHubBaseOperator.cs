using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System.Collections.Concurrent;
using Telegraphy.Azure.Exceptions;
using Microsoft.Azure.EventHubs;

namespace Telegraphy.Azure
{
    public class EventHubBaseOperator : IOperator
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private int maxDequeueCount = 1;
        private EventHubDataReciever EventHubMsgReciever;
        private EventHubDataDeliverer EventHubClient;
        private ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();

        internal EventHubBaseOperator(ILocalSwitchboard switchBoard, EventHubDataReciever eventHubMsgReciever, MessageSource messageSource = MessageSource.EntireIActor)
        {
            this.Switchboard = switchBoard;
            this.Switchboard.Operator = this;
            this.EventHubMsgReciever = eventHubMsgReciever;
            this.MessageSource = messageSource;
            this.ID = 0;
        }

        internal EventHubBaseOperator(EventHubDataDeliverer eventHubClient, MessageSource messageSource = MessageSource.EntireIActor)
        {
            this.EventHubClient = eventHubClient;
            this.MessageSource = messageSource;
            this.ID = 0;
        }

        public MessageSource MessageSource { get; private set; }

        private static IActorMessage ConvertToActorMessage(byte[] msgBytes, MessageSource messageSource)
        {
            IActorMessage msg = null;
            switch (messageSource)
            {
                case MessageSource.EntireIActor:
                    {
                        var t = Telegraph.Instance.Ask(new DeSerializeMessage(msgBytes));
                        msg = t.Result as IActorMessage;
                    }
                    break;

                case MessageSource.ByteArrayMessage:
                    msg = msgBytes.ToActorMessage(); break;
                case MessageSource.StringMessage:
                    msg = Encoding.UTF8.GetString(msgBytes).ToActorMessage(); break;
                default:
                    throw new NotImplementedException(messageSource.ToString());
            }
            return msg;
        }

        #region IOperator

        public long ID { get; set; }

        public ulong Count { get { return (ulong)msgQueue.Count; } }

        public ILocalSwitchboard Switchboard { get; set; }

        public void AddMessage(IActorMessage msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage(msg, this.MessageSource);
            EventHubClient.Send(eventData);
            
            if (null == msg.Status)
                msg.Status = new TaskCompletionSource<IActorMessage>();

            msg.Status?.SetResult(new EventHubMessage(eventData));
        }

        public IActorMessage GetMessage()
        {
            if (1 == maxDequeueCount)
            {
                var recievedSingle = this.EventHubMsgReciever.Recieve(maxDequeueCount, null);
                if (null == recievedSingle || 0 == recievedSingle.Count())
                    return null;

                byte[] msgBytes = null;
                msgBytes = recievedSingle.First().Body.Array;
                return ConvertToActorMessage(msgBytes, this.MessageSource);
            }

            while (0 != msgQueue.Count)
            {
                IActorMessage msg = null;
                if (msgQueue.TryDequeue(out msg))
                    return msg;
            }
           
            var recievedList = this.EventHubMsgReciever.Recieve(maxDequeueCount, null);
            if (null == recievedList || 0 == recievedList.Count())
                return null;

            for(int i=1; i < recievedList.Count(); ++i)
            {
                byte[] msgBytes = recievedList.ElementAt(i).Body.Array;
                msgQueue.Enqueue(ConvertToActorMessage(msgBytes, this.MessageSource));
            }

            // save the first one to return
            return ConvertToActorMessage(recievedList.ElementAt(0).Body.Array, this.MessageSource);
        }

        public bool IsAlive()
        {
            return null != this.Switchboard ? this.Switchboard.IsDisabled() : true;
        }

        public void Kill()
        {
            if (null != EventHubMsgReciever)
            {
                EventHubMsgReciever.Close();
            }

            this.Switchboard?.Disable();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            if (null == this.Switchboard && null != EventHubMsgReciever)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(Expression<Func<K>> factory)
            where T : class
            where K : IActor
        {
            if (null == this.Switchboard && null != EventHubMsgReciever)
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
            while ((DateTime.Now - start) < timeout && (0 != this.msgQueue.Count || 0 != this.EventHubMsgReciever.ApproximateCount()))
            {
                System.Threading.Thread.Sleep(1000);
            }

            return ((DateTime.Now - start) <= timeout);
        }
        #endregion

        #region IActor
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            AddMessage(msg);
            return true;
        }
        #endregion
    }
}
