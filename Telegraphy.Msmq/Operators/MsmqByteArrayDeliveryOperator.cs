using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqByteArrayDeliveryOperator : MsmqBaseOperator
    {
        public MsmqByteArrayDeliveryOperator(string queueName, string[] targetTypeNames)
               : this(".", queueName, targetTypeNames)
        {
        }

        public MsmqByteArrayDeliveryOperator(string machineName, string queueName, string[] targetTypeNames)
            : base(machineName, queueName, targetTypeNames, MessageSource.ByteArrayMessage)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
