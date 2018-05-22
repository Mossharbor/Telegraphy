using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Msmq;
using System.Collections.Concurrent;

namespace DurableSender
{
    class P
    {
        static int failCount = 0;
        static ConcurrentBag<Task> durableTasksToWaitOn = new ConcurrentBag<Task>();

        static void Main(string[] args)
        {
            // This sample show how a developer can use the microsoft queuing managment service (msmq) 
            // to store messages while offline in order to send them later.
            // We simulate being offline by throwing a WebException
            // We then send the message to the msmq as a store until we are back online (aka fix the default actor to not throw exceptions)
            // Once we are back online we retrieve the items from the durable queue and finish sending them to the default actor.

            // Turn msmq on on windows 10
            //     Open Control Panel.
            //     Click Programs and then, under Programs and Features, click Turn Windows Features on and off.
            //     Expand Microsoft Message Queue(MSMQ) Server, expand Microsoft Message Queue(MSMQ) Server Core, and then select the check boxes for the following Message Queuing features to install. ...
            //     Click OK.

            // send queued items to the 
            string durableQueueName = "DurableQueueName";
            Telegraph.Instance.MainOperator = new LocalOperator(); 
            string messageStr = "DurableSenderException.";
            DefaultActor da = new DefaultActor();
            da.OnMessageHandler = delegate (IActorMessage s) { throw new System.Net.WebException(); };

            Telegraph.Instance.Register<Messages.SendMsgToDurableQueue, MsmqDeliveryOperator<string>>(()=> new MsmqDeliveryOperator<string>(durableQueueName));
            Telegraph.Instance.Register<string, DefaultActor>(() => da);
            Telegraph.Instance.Register(typeof(System.Net.WebException), SendMessageToDurableQueueForLaterDelivery);

            int i = 0;
            for (; i < 10; ++i)
            {
                string msg = (messageStr + i.ToString());
                Telegraph.Instance.Tell(msg.ToActorMessage());
            }

            Console.WriteLine(string.Format("We sent {0} strings to the default actor", failCount));
            while (failCount != i)
                System.Threading.Thread.Sleep(100);

            Console.WriteLine(string.Format("{0} strings failed", failCount));
            Console.WriteLine(string.Format("We sent {0} strings to the durable queue", failCount));
            Task.WaitAll(durableTasksToWaitOn.ToArray());

            //Change the default actor to now process string messages
            int rerun = 0;
            da.OnMessageHandler = delegate (IActorMessage s) { Console.WriteLine((string)s.Message); ++rerun;  return true; };
            long receptionOp = Telegraph.Instance.Register(new MsmqReceptionOperator<string>(durableQueueName));
            Telegraph.Instance.Register<string, DefaultActor>(receptionOp, () => da);

            while (rerun <= i)
                System.Threading.Thread.Sleep(100);
            
            Console.WriteLine(string.Format("After fixing the default actor we sent {0} retrieved strings from the durable queue", failCount));
        }

        private static IActor SendMessageToDurableQueueForLaterDelivery(Exception exception, IActor actor, IActorMessage actorMessage, IActorInvocation actorInvocation)
        {
            ++failCount;
            durableTasksToWaitOn.Add(Telegraph.Instance.Ask(new Messages.SendMsgToDurableQueue(actorMessage)));
            return actor;
        }

        private class Messages
        {
            internal class SendMsgToDurableQueue : IActorMessage
            {
                public SendMsgToDurableQueue(IActorMessage msg) { this.Message = msg.Message;  }
                public object Message { get; set; }
                public object ProcessingResult { get; set; }
                public TaskCompletionSource<IActorMessage> Status { get; set; }
            }
        }
    }
}
