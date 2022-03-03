using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterSubscriptionOperator<T> : ServiceBusQueueBaseOperator<T> where T: class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicSubscriptionOperator<T>.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicSubscriptionOperator<T>.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicSubscriptionOperator<T>.DefaultConcurrency;

        public ServiceBusTopicDeadLetterSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicDeadLetterSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency)
               : base(new LocalSwitchboard(concurrencyType, concurrency), GetSender(connectionString, topicName, subscriptionName), maxDequeueCount)
        {
        }

        public ServiceBusTopicDeadLetterSubscriptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount)
               : base(switchBoard, GetSender(connectionString, topicName, subscriptionName), maxDequeueCount)
        {
        }

        internal static ServiceBusDeadLetterQueue GetSender(string connectionString, string topicName, string subscriptionName)
        {
            if (!String.IsNullOrWhiteSpace(subscriptionName))
                return new ServiceBusDeadLetterQueue(connectionString, topicName, subscriptionName);
            else
                return new ServiceBusDeadLetterQueue(connectionString, topicName);
        }
    }
}
