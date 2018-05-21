using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterReceptionOperator : ServiceBusTopicBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicReceptionOperator.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicReceptionOperator.DefaultConcurrency;

        public ServiceBusTopicDeadLetterReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicDeadLetterReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusTopicDeadLetterActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }

        public ServiceBusTopicDeadLetterReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, ServiceBusTopicDeadLetterActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }
    }
}
