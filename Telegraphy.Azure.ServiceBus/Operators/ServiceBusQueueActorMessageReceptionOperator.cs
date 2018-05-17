using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    using Mossharbor.AzureWorkArounds.ServiceBus;

    public class ServiceBusQueueActorMessageReceptionOperator : ServiceBusQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusQueueActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount) 
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount) 
            : this(new LocalSwitchboard(concurrencyType, concurrency), new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueActorMessageReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount) 
            : this(switchBoard, ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueActorMessageReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount)
           : this(switchBoard, new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        {
        }

        private ServiceBusQueueActorMessageReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount) 
            : base(switchBoard, queue, maxDequeueCount,MessageSource.EntireIActor)
        {
        }
    }
}
