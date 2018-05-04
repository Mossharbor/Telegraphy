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
        public ServiceBusTopicDeadLetterReciever(string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, ReceiveMode receiveMode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null, int prefetchCount = 0)
               : base(connectionString, EntityNameHelper.FormatDeadLetterPath(EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName)), createQueueIfItDoesNotExist, receiveMode, retryPolicy, prefetchCount)
        {
            this.topic = topicName;
            this.subscription = subscriptionName;
        }
    }
}
