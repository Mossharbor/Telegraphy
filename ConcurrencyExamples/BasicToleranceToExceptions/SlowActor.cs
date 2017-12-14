using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicToleranceToFailure
{
    using Telegraphy.Net;

    class SlowActor : DefaultActor
    {
        public SlowActor()
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
            Console.WriteLine("Sleeping for 10 seconds");
            System.Threading.Thread.Sleep(100000);

            Console.WriteLine(((IActorMessage)message).Message.ToString());
            return true;
        }
    }
}
