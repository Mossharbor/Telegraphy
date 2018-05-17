using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterByteArrayDeliveryOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueDeadLetterByteArrayDeliveryOperator(string connectionString, string queueName)
            : base(ServiceBusQueueDeadLetterActorMessageDeliveryOperator.GetQueue(connectionString, queueName), Telegraphy.Net.MessageSource.ByteArrayMessage)
        { }

        public ServiceBusQueueDeadLetterByteArrayDeliveryOperator(QueueClient queue, string connectionString)
            : base(new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), Telegraphy.Net.MessageSource.ByteArrayMessage)
        { }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
