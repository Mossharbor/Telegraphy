using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicReceptionOperator : ServiceBusTopicBaseOperator
    {
        internal const int DefaultDequeueMaxCount = 3;
        internal const int DefaultPrefetchCount = 0;
        internal const int DefaultConcurrency = 1;

        public ServiceBusTopicReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusTopicActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }

        public ServiceBusTopicReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, ServiceBusTopicActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }
    }
}
