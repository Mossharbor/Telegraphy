using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterSubscriptionOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicSubscriptionOperator<T>.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicSubscriptionOperator<T>.DefaultConcurrency;

        public ServiceBusQueueDeadLetterSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueueDeadLetterPublishOperator<T>.GetQueue(connectionString, queueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterSubscriptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterSubscriptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(switchBoard, ServiceBusQueueDeadLetterPublishOperator<T>.GetQueue(connectionString, queueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterSubscriptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, int maxDequeueCount = DefaultDequeueMaxCount)
           : this(switchBoard, new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), maxDequeueCount)
        {
        }

        private ServiceBusQueueDeadLetterSubscriptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : base(switchBoard, queue, maxDequeueCount)
        {
        }
    }
}
