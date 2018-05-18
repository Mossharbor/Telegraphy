using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqActorMessageDeliveryOperator : MsmqBaseOperator
    {
        public MsmqActorMessageDeliveryOperator(string queueName, string[] targetTypeNames)
               : this(".", queueName, targetTypeNames)
        {
        }

        public MsmqActorMessageDeliveryOperator(string machineName, string queueName, string[] targetTypeNames) 
            : base(machineName, queueName, targetTypeNames, MessageSource.EntireIActor)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
