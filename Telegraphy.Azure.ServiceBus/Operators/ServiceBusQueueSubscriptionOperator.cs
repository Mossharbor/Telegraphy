using global::Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueSubscriptionOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicSubscriptionOperator<T>.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicSubscriptionOperator<T>.DefaultConcurrency;

        public ServiceBusQueueSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueuePublishOperator<T>.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        public ServiceBusQueueSubscriptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(switchBoard, ServiceBusQueuePublishOperator<T>.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), maxDequeueCount)
        { }

        private ServiceBusQueueSubscriptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : base(switchBoard, queue, maxDequeueCount)
        {
        }
    }
}
