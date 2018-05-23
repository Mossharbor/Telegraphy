using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicPublishOperator <T>: ServiceBusTopicBaseOperator<T> where T: class
    {
        public ServiceBusTopicPublishOperator(string connectionString, string topicName, string subscription, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : this(connectionString, topicName, new string[] { subscription }, createTopicIfItDoesNotExist, policy)
        {
        }

        public ServiceBusTopicPublishOperator(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(ServiceBusTopicPublishOperator<T>.GetSender(connectionString, topicName, createTopicIfItDoesNotExist, policy))
        {
        }

        public ServiceBusTopicPublishOperator(string connectionString, string topicName, string[] subscriptionNames, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(ServiceBusTopicPublishOperator<T>.GetSender(connectionString, topicName, subscriptionNames, createTopicIfItDoesNotExist, policy))
        {
        }

        internal static ServiceBusTopicDeliverer GetSender(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
        {
            return new ServiceBusTopicDeliverer(connectionString, topicName, createTopicIfItDoesNotExist, policy);
        }

        internal static ServiceBusTopicDeliverer GetSender(string connectionString, string topicName, string[] subscriptionNames, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
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
