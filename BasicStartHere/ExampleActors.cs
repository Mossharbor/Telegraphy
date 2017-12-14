using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicStartHere
{
    using Telegraphy.Net;

    public class LazyInstantiationActor : DefaultActor
    {
        public LazyInstantiationActor()
        {
            this.OnMessageHandler = Print;
            this.OnExitHandler = Exit;
        }

        public bool Exit(IActorMessage message)
        {
            //if (msg is HangUp) { /* Do Some cleanup */}
            
            return true;
        }

        public bool Print(IActorMessage message)
        {
            if (message.Message is byte[])
            {
                Console.WriteLine(Encoding.ASCII.GetString((byte[])message.Message));
                return true;
            }
            else
            {
                Console.WriteLine(((IActorMessage)message).Message.ToString());
                return true;
            }
        }
    }
}
