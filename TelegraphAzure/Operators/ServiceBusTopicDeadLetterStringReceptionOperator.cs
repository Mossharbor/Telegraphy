using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterStringReceptionOperator : ServiceBusTopicBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicActorMessageReceptionOperator.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusTopicDeadLetterStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicDeadLetterStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusTopicDeadLetterActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, MessageSource.StringMessage)
        {
        }

        public ServiceBusTopicDeadLetterStringReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, ServiceBusTopicDeadLetterActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, prefetchCount, policy), maxDequeueCount, MessageSource.StringMessage)
        {
        }
    }
}
