using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicDeliveryOperator : ServiceBusTopicOperator
    {
        public ServiceBusTopicDeliveryOperator(string connectionString, string topicName = "", Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : this(GetSender(connectionString, topicName, policy))
        {
        }

        public ServiceBusTopicDeliveryOperator(Microsoft.Azure.ServiceBus.Core.MessageSender sender)
            : base(sender)
        {
        }

        private static Microsoft.Azure.ServiceBus.Core.MessageSender GetSender(string connectionString, string topicName, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
        {
            return new Microsoft.Azure.ServiceBus.Core.MessageSender(connectionString, topicName, policy);
        }
    }
}
