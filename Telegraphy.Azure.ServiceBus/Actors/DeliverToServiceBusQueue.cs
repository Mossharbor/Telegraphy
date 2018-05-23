using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class DeliverToServiceBusQueue<MsgType> : IActor where MsgType:class
    {
        ServiceBusQueue queue = null;

        public DeliverToServiceBusQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusQueuePublishOperator<MsgType>.GetQueue(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusQueueBaseOperator<MsgType>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
