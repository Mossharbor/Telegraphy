using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public class DefaultActor : IActor
    {
        public DefaultActor()
        {
        }

        public DefaultActor(Func<IActorMessage,bool> msgFcn)
        {
            this.OnMessageHandler = msgFcn;
        }

        public Func<IActorMessage,bool> OnMessageHandler { get; set; }

        public Func<IActorMessage,bool> OnExitHandler { get; set; }

        #region IActor

        public virtual bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
            if (typeof(HangUp) == msg.GetType())
            {
                if (null != OnExitHandler)
                    return OnExitHandler(msg);

                return true;
            }

            return (null == OnMessageHandler) ? false : OnMessageHandler(msg);
        }

        #endregion
    }
}
