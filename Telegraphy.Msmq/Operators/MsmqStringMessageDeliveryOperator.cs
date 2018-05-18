using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqStringMessageDeliveryOperator : MsmqBaseOperator
    {
        public MsmqStringMessageDeliveryOperator(string queueName, string[] targetTypeNames)
               : this(".", queueName, targetTypeNames)
        {
        }

        public MsmqStringMessageDeliveryOperator(string machineName, string queueName, string[] targetTypeNames)
            : base(machineName, queueName, targetTypeNames, MessageSource.StringMessage)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
