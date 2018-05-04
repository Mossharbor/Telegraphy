using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicStringDeliveryOperator : ServiceBusTopicBaseOperator
    {
        public ServiceBusTopicStringDeliveryOperator(string connectionString, string topicName, string subscription, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : this(connectionString, topicName, new string[] { subscription }, createTopicIfItDoesNotExist, policy)
        {
        }

        public ServiceBusTopicStringDeliveryOperator(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(ServiceBusTopicActorMessageDeliveryOperator.GetSender(connectionString, topicName, createTopicIfItDoesNotExist, policy), MessageSource.StringMessage)
        {
        }

        public ServiceBusTopicStringDeliveryOperator(string connectionString, string topicName, string[] subscriptionNames, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(ServiceBusTopicActorMessageDeliveryOperator.GetSender(connectionString, topicName, subscriptionNames, createTopicIfItDoesNotExist, policy), MessageSource.StringMessage)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
