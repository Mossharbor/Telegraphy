using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToServiceBusTopic : DeliverToServiceBusTopic<byte[]>
    { 
        public SendBytesToServiceBusTopic(string storageConnectionString, string topicName, bool createQueueIfItDoesNotExist = true)
            :base(storageConnectionString, topicName, createQueueIfItDoesNotExist)
        {
        }
    }
}
