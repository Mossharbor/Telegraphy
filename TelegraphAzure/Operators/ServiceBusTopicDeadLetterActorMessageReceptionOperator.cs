using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    class ServiceBusTopicDeadLetterActorMessageReceptionOperator : ServiceBusTopicBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicActorMessageReceptionOperator.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusTopicDeadLetterActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicDeadLetterActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, MessageSource.EntireIActor)
        {
        }

        public ServiceBusTopicDeadLetterActorMessageReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, MessageSource.EntireIActor)
        {
        }

        internal static ServiceBusTopicDeadLetterReciever GetSender(string connectionString, string topicName, string subscription, int prefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy)
        {
            return new ServiceBusTopicDeadLetterReciever(connectionString, topicName, subscription, prefetchCount, policy);
        }
    }
}
