using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendMessageToServiceBusQueue : IActor
    {
        ServiceBusQueue queue = null;

        public SendMessageToServiceBusQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = ServiceBusQueueActorMessageDeliveryOperator.GetQueue(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ServiceBusQueueBaseOperator.SerializeAndSend(msg, queue, Telegraphy.Net.MessageSource.EntireIActor);
            return true;
        }
    }
}
