using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendStringToServiceBusTopic : IActor
    {
        ServiceBusTopicDeliverer queue = null;

        public SendStringToServiceBusTopic(string storageConnectionString, string topicName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusTopicDeliveryOperator<string>.GetSender(storageConnectionString, topicName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusTopicBaseOperator<string>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
