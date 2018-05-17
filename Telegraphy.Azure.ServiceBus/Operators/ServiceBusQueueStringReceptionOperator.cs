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
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueStringReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(switchBoard, ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueStringReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount)
           : this(switchBoard, new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        {
        }

        private ServiceBusQueueStringReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : base(switchBoard, queue, maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }
    }
}
