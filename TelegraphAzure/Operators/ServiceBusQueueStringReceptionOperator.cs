using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.ServiceBus;
    using Telegraphy.Net;

    public class ServiceBusQueueStringReceptionOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = 1, int maxDequeueCount = 3)
            : this(concurrencyType, ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist, concurrency, maxDequeueCount)
        { }

        public ServiceBusQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, uint concurrency = 1, int maxDequeueCount = 3)
            : this(new LocalSwitchboard(concurrencyType, concurrency), queue, connectionString, createQueueIfItDoesNotExist, maxDequeueCount)
        { }

        public ServiceBusQueueStringReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3)
            : this(switchBoard, ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist, maxDequeueCount)
        { }

        public ServiceBusQueueStringReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3)
           : this(switchBoard, new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        {
        }

        private ServiceBusQueueStringReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount = 3)
            : base(switchBoard, queue, maxDequeueCount, MessageSource.StringMessage)
        {
        }
    }
}
