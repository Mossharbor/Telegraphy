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
        public MsmqStringMessageDeliveryOperator(string queueName)
               : this(".", queueName)
        {
        }

        public MsmqStringMessageDeliveryOperator(string machineName, string queueName)
            : base(machineName, queueName, MessageSource.StringMessage)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
