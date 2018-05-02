﻿using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeliveryOperator : ServiceBusTopicOperator
    {
        public ServiceBusTopicDeliveryOperator(string connectionString, string topicName,string subscription, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : this(connectionString, topicName, new string[] { subscription }, createTopicIfItDoesNotExist, policy)
        {
        }

        public ServiceBusTopicDeliveryOperator(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, createTopicIfItDoesNotExist, policy))
        {
        }

        public ServiceBusTopicDeliveryOperator(string connectionString, string topicName, string[] subscriptionNames, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, subscriptionNames, createTopicIfItDoesNotExist, policy))
        {
        }

        private static ServiceBusTopicDeliverer GetSender(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
        {
            return new ServiceBusTopicDeliverer(connectionString, topicName, createTopicIfItDoesNotExist, policy);
        }

        private static ServiceBusTopicDeliverer GetSender(string connectionString, string topicName, string[] subscriptionNames, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
        {
            return new ServiceBusTopicDeliverer(connectionString, topicName, subscriptionNames, createTopicIfItDoesNotExist, policy);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
