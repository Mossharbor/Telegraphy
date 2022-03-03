using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class DeliverToServiceBusTopic<MsgType> : IActor where MsgType : class
    {
        ServiceBusSender sender;

        public DeliverToServiceBusTopic(string connectionString, string topicName, bool createQueueIfItDoesNotExist = true)
        {
            // handles create if not exists
            var queue = ServiceBusTopicPublishOperator<MsgType>.GetTopic(connectionString, topicName, createQueueIfItDoesNotExist);
            sender = new ServiceBusClient(connectionString).CreateSender(topicName);
        }

        public DeliverToServiceBusTopic(string connectionString, string topicName, string subscription, bool createQueueIfItDoesNotExist = true)
        {
            // handles create if not exists
            var queue = ServiceBusTopicPublishOperator<MsgType>.GetTopic(connectionString, topicName, subscription, createQueueIfItDoesNotExist);
            sender = new ServiceBusClient(connectionString).CreateSender(topicName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusQueueBaseOperator<MsgType>.SerializeAndSend(msg, sender);
            return true;
        }
    }
}
