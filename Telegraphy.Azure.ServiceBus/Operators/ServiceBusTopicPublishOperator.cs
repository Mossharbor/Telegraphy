using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicPublishOperator <T>: ServiceBusQueueBaseOperator<T> where T: class
    {
        public ServiceBusTopicPublishOperator(string connectionString, string topicName, string subscription, bool createTopicIfItDoesNotExist)
               : base(ServiceBusTopicPublishOperator<T>.GetTopic(connectionString, topicName, subscription, createTopicIfItDoesNotExist))
        {
        }

        public ServiceBusTopicPublishOperator(string connectionString, string topicName, bool createTopicIfItDoesNotExist)
               : base(ServiceBusTopicPublishOperator<T>.GetTopic(connectionString, topicName, createTopicIfItDoesNotExist))
        {
        }
        internal static ServiceBusQueue GetTopic(string connectionString, string topicName, bool createTopicIfItDoesNotExist)
        {
            return new ServiceBusQueue(connectionString, topicName, null, ServiceBusReceiveMode.PeekLock, createTopicIfItDoesNotExist);
        }

        internal static ServiceBusQueue GetTopic(string connectionString, string topicName, string subscriptionName, bool createTopicIfItDoesNotExist)
        {
            return new ServiceBusQueue(connectionString, topicName, subscriptionName, ServiceBusReceiveMode.PeekLock, createTopicIfItDoesNotExist);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
