using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeadLetterActorMessageDeliveryOperator : ServiceBusTopicBaseOperator
    {
        public ServiceBusTopicDeadLetterActorMessageDeliveryOperator(string connectionString, string topicName, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, null,  policy), MessageSource.EntireIActor)
        {
        }

        public ServiceBusTopicDeadLetterActorMessageDeliveryOperator(string connectionString, string topicName, string subscriptionName, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(GetSender(connectionString, topicName, subscriptionName, policy), MessageSource.EntireIActor)
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
