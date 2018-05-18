using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqByteArrayMessageDeliveryOperator : MsmqBaseOperator
    {
        public MsmqByteArrayMessageDeliveryOperator(string queueName)
               : this(".", queueName)
        {
        }

        public MsmqByteArrayMessageDeliveryOperator(string machineName, string queueName)
            : base(machineName, queueName, MessageSource.ByteArrayMessage)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
