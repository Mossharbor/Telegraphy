using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterSubscriptionOperator<T> : ServiceBusTopicBaseOperator<T> where T: class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicSubscriptionOperator<T>.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicSubscriptionOperator<T>.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicSubscriptionOperator<T>.DefaultConcurrency;

        public ServiceBusTopicDeadLetterSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicDeadLetterSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount)
        {
        }

        public ServiceBusTopicDeadLetterSubscriptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount)
        {
        }

        internal static ServiceBusTopicDeadLetterReciever GetSender(string connectionString, string topicName, string subscription, int prefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy)
        {
            return new ServiceBusTopicDeadLetterReciever(connectionString, topicName, subscription, prefetchCount, policy);
        }
    }
}
