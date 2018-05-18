using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    internal class AnonActor<MsgType> : IActor, IAnonActor where MsgType : class
    {
        Action<MsgType> onTellAction = null;

        public AnonActor(Action<MsgType> onTellAction)
        {
            this.onTellAction = onTellAction;
        }

        #region IActor
        public bool OnMessageRecieved(MsgType msg)
        {
            if (typeof(ControlMessages.HangUp) == msg.GetType())
                return true;
            
            onTellAction(msg);
            return true;
        }

        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            if (typeof(ControlMessages.HangUp) == msg.GetType())
                return true;

            if (msg is MsgType)
                OnMessageRecieved(msg as MsgType);
            else
                onTellAction((MsgType)msg.Message);
            return true;
        }

        #endregion

    }
}
