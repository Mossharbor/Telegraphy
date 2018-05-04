using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueByteArrayDeliveryOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueByteArrayDeliveryOperator(string connectionString, string queueName, bool createQueueIfItDoesNotExist)
            : base(ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), MessageSource.ByteArrayMessage)
        { }

        public ServiceBusQueueByteArrayDeliveryOperator(QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist)
            : base(new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), MessageSource.ByteArrayMessage)
        { }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
