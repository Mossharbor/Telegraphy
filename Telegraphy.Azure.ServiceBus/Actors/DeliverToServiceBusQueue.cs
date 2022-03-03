using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class DeliverToServiceBusQueue<MsgType> : IActor where MsgType:class
    {
        ServiceBusSender sender = null;

        public DeliverToServiceBusQueue(string connectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            // handles create if not exists
            var queue = ServiceBusQueuePublishOperator<MsgType>.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist);
            sender = new ServiceBusClient(connectionString).CreateSender(queueName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusQueueBaseOperator<MsgType>.SerializeAndSend(msg, sender);
            return true;
        }
    }
}
