using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicActorMessageDeliveryOperator : ServiceBusTopicBaseOperator
    {
        public ServiceBusTopicActorMessageDeliveryOperator(string connectionString, string topicName,string subscription, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : this(connectionString, topicName, new string[] { subscription }, createTopicIfItDoesNotExist, policy)
        {
        }

        public ServiceBusTopicActorMessageDeliveryOperator(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, createTopicIfItDoesNotExist, policy), Telegraphy.Net.MessageSource.EntireIActor)
        {
        }

        public ServiceBusTopicActorMessageDeliveryOperator(string connectionString, string topicName, string[] subscriptionNames, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, subscriptionNames, createTopicIfItDoesNotExist, policy), Telegraphy.Net.MessageSource.EntireIActor)
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
