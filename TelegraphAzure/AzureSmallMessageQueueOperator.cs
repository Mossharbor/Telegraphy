using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class AzureSmallMessageQueueOperator : IOperator
    {
        public AzureSmallMessageQueueOperator(ILocalSwitchboard switchBoard)
        {
            this.ID = 0;
        }

        #region IActor

        public bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IOperator

        public long ID { get; set; }

        public bool IsAlive()
        {
            throw new NotImplementedException();
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }

        public ulong Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ILocalSwitchboard Switchboard { get; set; }

        public void AddMessage(IActorMessage msg)
        {
            throw new NotImplementedException();
        }

        public IActorMessage GetMessage()
        {
            throw new NotImplementedException();
        }

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void Register<T>(Action<T> action)
        {
            throw new NotImplementedException();
        }

        public void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
            //where T : IActorMessage
            where K : IActor
        {
            throw new NotImplementedException();
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
