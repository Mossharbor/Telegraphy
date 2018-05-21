using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterReceptionOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicReceptionOperator<T>.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicReceptionOperator<T>.DefaultConcurrency;

        public ServiceBusQueueDeadLetterReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueueDeadLetterDeliveryOperator<T>.GetQueue(connectionString, queueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(switchBoard, ServiceBusQueueDeadLetterDeliveryOperator<T>.GetQueue(connectionString, queueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, int maxDequeueCount = DefaultDequeueMaxCount)
           : this(switchBoard, new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), maxDequeueCount)
        {
        }

        private ServiceBusQueueDeadLetterReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : base(switchBoard, queue, maxDequeueCount)
        {
        }
    }
}
