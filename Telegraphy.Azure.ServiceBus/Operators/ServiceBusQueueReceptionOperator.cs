using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueReceptionOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusQueueReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueueDeliveryOperator<T>.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(switchBoard, ServiceBusQueueDeliveryOperator<T>.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount)
           : this(switchBoard, new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        {
        }

        private ServiceBusQueueReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : base(switchBoard, queue, maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }
    }
}
