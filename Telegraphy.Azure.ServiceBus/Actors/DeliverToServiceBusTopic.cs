using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class DeliverToServiceBusTopic<MsgType> : IActor where MsgType : class
    {
        ServiceBusTopicDeliverer queue = null;

        public DeliverToServiceBusTopic(string storageConnectionString, string topicName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusTopicPublishOperator<MsgType>.GetSender(storageConnectionString, topicName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusTopicBaseOperator<MsgType>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
