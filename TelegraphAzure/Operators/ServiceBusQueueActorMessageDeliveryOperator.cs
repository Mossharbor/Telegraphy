using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.ServiceBus;

    public class ServiceBusQueueActorMessageDeliveryOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueActorMessageDeliveryOperator(string connectionString,string queueName, bool createQueueIfItDoesNotExist)
            : this(GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist)
        { }

        public ServiceBusQueueActorMessageDeliveryOperator(QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist)
            : base (new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist),MessageSource.EntireIActor)
        { }

        internal static QueueClient GetQueue(string connectionString,string queueName)
        {
            return new QueueClient(connectionString, queueName);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
