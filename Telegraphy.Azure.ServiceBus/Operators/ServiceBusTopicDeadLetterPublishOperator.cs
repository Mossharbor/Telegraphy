using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterPublishOperator<T> : ServiceBusQueueBaseOperator<T> where T:class
    {
        public ServiceBusTopicDeadLetterPublishOperator(string connectionString, string topicName)
               : base(GetSender(connectionString, topicName, null))
        {
        }

        public ServiceBusTopicDeadLetterPublishOperator(string connectionString, string topicName, string subscriptionName)
               : base(GetSender(connectionString, topicName, subscriptionName))
        {
        }

        internal static ServiceBusDeadLetterQueue GetSender(string connectionString, string topicName, string subscriptionName)
        {
            if (!String.IsNullOrWhiteSpace(subscriptionName))
                return new ServiceBusDeadLetterQueue(connectionString, topicName, subscriptionName);
            else
                return new ServiceBusDeadLetterQueue(connectionString, topicName);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
