using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterPublishOperator<T> : ServiceBusTopicBaseOperator<T> where T:class
    {
        public ServiceBusTopicDeadLetterPublishOperator(string connectionString, string topicName, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, null, policy))
        {
        }

        public ServiceBusTopicDeadLetterPublishOperator(string connectionString, string topicName, string subscriptionName, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, subscriptionName, policy))
        {
        }

        internal static ServiceBusTopicDeadLetterDeliverer GetSender(string connectionString, string topicName, string subscriptionName, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
        {
            if (!String.IsNullOrWhiteSpace(subscriptionName))
                return new ServiceBusTopicDeadLetterDeliverer(connectionString, topicName, subscriptionName, policy);
            else
                return new ServiceBusTopicDeadLetterDeliverer(connectionString, topicName, policy);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
