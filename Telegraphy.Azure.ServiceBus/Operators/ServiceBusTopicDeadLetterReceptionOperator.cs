using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterReceptionOperator<T> : ServiceBusTopicBaseOperator<T> where T: class
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicReceptionOperator<T>.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicReceptionOperator<T>.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicReceptionOperator<T>.DefaultConcurrency;

        public ServiceBusTopicDeadLetterReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicDeadLetterReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }

        public ServiceBusTopicDeadLetterReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.StringMessage)
        {
        }

        internal static ServiceBusTopicDeadLetterReciever GetSender(string connectionString, string topicName, string subscription, int prefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy)
        {
            return new ServiceBusTopicDeadLetterReciever(connectionString, topicName, subscription, prefetchCount, policy);
        }
    }
}
