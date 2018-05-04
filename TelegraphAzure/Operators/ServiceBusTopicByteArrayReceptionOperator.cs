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
        const int DefaultDequeueMaxCount = 3;
        const int DefaultPrefetchCount = 0;
        const int DefaultConcurrency = 1;

        public ServiceBusTopicByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusTopicActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, prefetchCount, policy), maxDequeueCount, MessageSource.ByteArrayMessage)
        {
        }

        public ServiceBusTopicByteArrayReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, ServiceBusTopicActorMessageReceptionOperator.GetSender(connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, prefetchCount, policy), maxDequeueCount, MessageSource.ByteArrayMessage)
        {
        }
    }
}
