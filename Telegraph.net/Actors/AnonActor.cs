using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    internal class AnonActor<MsgType> : IActor, IAnonActor
    {
        Action<MsgType> onTellAction = null;

        public AnonActor(Action<MsgType> onTellAction)
        {
            this.onTellAction = onTellAction;
        }

        #region IActor

        public bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
            if (typeof(HangUp) == msg.GetType())
                return true;

            onTellAction((MsgType)msg.Message);
            return true;
        }

        #endregion

    }
}
