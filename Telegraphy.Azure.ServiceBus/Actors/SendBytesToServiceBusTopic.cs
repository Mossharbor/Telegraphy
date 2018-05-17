using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToServiceBusTopic : IActor
    {
        ServiceBusTopicDeliverer queue = null;

        public SendBytesToServiceBusTopic(string storageConnectionString, string topicName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusTopicActorMessageDeliveryOperator.GetSender(storageConnectionString, topicName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusTopicBaseOperator.SerializeAndSend(msg, queue, Telegraphy.Net.MessageSource.ByteArrayMessage);
            return true;
        }
    }
}
