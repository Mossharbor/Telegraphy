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
        public ServiceBusTopicDeadLetterDeliverer(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy retryPolicy = null)
              : base(connectionString, EntityNameHelper.FormatDeadLetterPath(topicName), createTopicIfItDoesNotExist, retryPolicy)
        {
        }

    }
}
