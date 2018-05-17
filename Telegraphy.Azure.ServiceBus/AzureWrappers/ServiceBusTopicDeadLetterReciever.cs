using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusTopicDeadLetterReciever : ServiceBusTopicReciever
    {
        public ServiceBusTopicDeadLetterReciever(string connectionString, string topicName, string subscriptionName, int prefetchCount = 0, RetryPolicy retryPolicy = null, ReceiveMode receiveMode = ReceiveMode.PeekLock)
               : base(connectionString, EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName)), false, receiveMode, retryPolicy, prefetchCount)
        {
            this.topic = topicName;
            this.subscription = subscriptionName;
        }

        public ServiceBusTopicDeadLetterReciever(string connectionString, string topicName, int prefetchCount = 0, RetryPolicy retryPolicy = null, ReceiveMode receiveMode = ReceiveMode.PeekLock)
               : base(connectionString, EntityNameHelper.FormatDeadLetterPath(topicName), false, receiveMode, retryPolicy, prefetchCount)
        {
            this.topic = topicName;
            this.subscription = null;
        }
    }
}
