using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendMessageToServiceBusTopic : DeliverToServiceBusTopic<IActorMessage>
    {
        public SendMessageToServiceBusTopic(string storageConnectionString, string topicName, bool createQueueIfItDoesNotExist = true)
            : base(storageConnectionString, topicName, createQueueIfItDoesNotExist)
        {
        }
    }
}
