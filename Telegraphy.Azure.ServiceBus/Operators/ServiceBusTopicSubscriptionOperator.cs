using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicSubscriptionOperator<T> : ServiceBusQueueBaseOperator<T> where T:class
    {
        internal const int DefaultDequeueMaxCount = 3;
        internal const int DefaultPrefetchCount = 0;
        internal const int DefaultConcurrency = 1;

        public ServiceBusTopicSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscription, bool createTopicIfItDoesNotExist)
               : base(new LocalSwitchboard(concurrencyType, DefaultConcurrency), ServiceBusTopicPublishOperator<T>.GetTopic(connectionString, topicName, subscription, createTopicIfItDoesNotExist), DefaultDequeueMaxCount)
        {
        }

        public ServiceBusTopicSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, bool createTopicIfItDoesNotExist)
               : base(new LocalSwitchboard(concurrencyType, DefaultConcurrency), ServiceBusTopicPublishOperator<T>.GetTopic(connectionString, topicName, createTopicIfItDoesNotExist), DefaultDequeueMaxCount)
        {
        }

        public ServiceBusTopicSubscriptionOperator(ILocalSwitchboard switchboard, string connectionString, string topicName, string subscription, bool createTopicIfItDoesNotExist)
               : base(switchboard, ServiceBusTopicPublishOperator<T>.GetTopic(connectionString, topicName, subscription, createTopicIfItDoesNotExist), DefaultDequeueMaxCount)
        {
        }

        public ServiceBusTopicSubscriptionOperator(ILocalSwitchboard switchboard, string connectionString, string topicName, bool createTopicIfItDoesNotExist)
               : base(switchboard, ServiceBusTopicPublishOperator<T>.GetTopic(connectionString, topicName, createTopicIfItDoesNotExist), DefaultDequeueMaxCount)
        {
        }

        internal static ServiceBusQueue GetSender(string connectionString, string topicName, bool createTopicIfItDoesNotExist)
        {
            return new ServiceBusQueue(connectionString, topicName, ServiceBusReceiveMode.PeekLock, createTopicIfItDoesNotExist);
        }

        internal static ServiceBusQueue GetSender(string connectionString, string topicName, string subscriptionName, bool createTopicIfItDoesNotExist)
        {
            return new ServiceBusQueue(connectionString, topicName, subscriptionName, ServiceBusReceiveMode.PeekLock, createTopicIfItDoesNotExist);
        }
    }
}
