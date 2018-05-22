using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicReceptionOperator<T> : ServiceBusTopicBaseOperator<T> where T:class
    {
        internal const int DefaultDequeueMaxCount = 3;
        internal const int DefaultPrefetchCount = 0;
        internal const int DefaultConcurrency = 1;

        public ServiceBusTopicReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), GetSender(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, prefetchCount, policy), maxDequeueCount)
        {
        }

        public ServiceBusTopicReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, GetSender(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, prefetchCount, policy), maxDequeueCount)
        {
        }

        internal static ServiceBusTopicReciever GetSender(string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy retryPolicy = null, Microsoft.Azure.ServiceBus.ReceiveMode receiveMode = Microsoft.Azure.ServiceBus.ReceiveMode.PeekLock)
        {
            return new ServiceBusTopicReciever(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, receiveMode, retryPolicy, prefetchCount);
        }
    }
}
