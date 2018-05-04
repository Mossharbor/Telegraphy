﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    class SendBytesToServiceBusQueue : IActor
    {
        ServiceBusQueue queue = null;

        public SendBytesToServiceBusQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusQueueActorMessageDeliveryOperator.GetQueue(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusQueueBaseOperator.SerializeAndSend(msg, queue, MessageSource.ByteArrayMessage);
            return true;
        }
    }
}