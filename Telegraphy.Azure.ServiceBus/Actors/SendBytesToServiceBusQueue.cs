using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToServiceBusQueue : IActor
    {
        ServiceBusQueue queue = null;

        public SendBytesToServiceBusQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusQueueActorMessageDeliveryOperator.GetQueue(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusQueueBaseOperator<byte[]>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
