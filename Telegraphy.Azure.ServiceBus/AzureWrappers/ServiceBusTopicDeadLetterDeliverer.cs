
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusTopicDeadLetterDeliverer : ServiceBusTopicDeliverer
    {
        public ServiceBusTopicDeadLetterDeliverer(string connectionString, string topicName, RetryPolicy retryPolicy = null)
              : base(connectionString, EntityNameHelper.FormatDeadLetterPath(topicName), false, retryPolicy)
        {
            this.topic = topicName;
        }

        public ServiceBusTopicDeadLetterDeliverer(string connectionString, string topicName, string subscriptionName, RetryPolicy retryPolicy = null)
            : base(connectionString, EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName)), false, retryPolicy)
        {
            this.topic = topicName;
            this.subscriptions = new string[] { subscriptionName };
        }
    }
}
