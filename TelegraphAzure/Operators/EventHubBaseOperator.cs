using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public abstract class EventHubBaseOperator : IOperator
    {
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        EventHubClient eventHubClient;
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;

        #region IOperator
        public long ID { get; set; }

        public ulong Count => throw new NotImplementedException();

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

            try
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage(msg));
                byte[] msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
                EventData eventMsg = new EventData(msgBytes);

                this.eventHubClient.SendAsync(eventMsg).Wait();

                if (null != msg.Status)
                    msg.Status?.SetResult(new EventHubMessage(eventMsg));
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
            if (null == this.Switchboard && !recieveMessagesOnly)
                throw new CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException();

            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
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

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            throw new NotImplementedException();
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
