﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicByteArrayReceptionOperator : ServiceBusTopicBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultPrefetchCount = ServiceBusTopicActorMessageReceptionOperator.DefaultPrefetchCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusTopicByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusTopicActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.ByteArrayMessage)
        {
        }

        public ServiceBusTopicByteArrayReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, bool createTopicAndSubscriptionIfTheyDoNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, ServiceBusTopicActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, createTopicAndSubscriptionIfTheyDoNotExist, prefetchCount, policy), maxDequeueCount, Telegraphy.Net.MessageSource.ByteArrayMessage)
        {
        }
    }
}