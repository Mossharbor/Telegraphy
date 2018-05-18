using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqDeliveryOperator<T> : MsmqBaseOperator<T> where T:class
    {
        public MsmqDeliveryOperator(string queueName)
               : this(".", queueName)
        {
        }

        public MsmqDeliveryOperator(string machineName, string queueName)
            : base(machineName, queueName)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
