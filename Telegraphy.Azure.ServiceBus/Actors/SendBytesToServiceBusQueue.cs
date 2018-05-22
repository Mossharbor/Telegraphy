using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToServiceBusQueue : DeliverToServiceBusQueue<byte[]>
    {
        public SendBytesToServiceBusQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
            :base(storageConnectionString, queueName, createQueueIfItDoesNotExist)
        {}
    }
}
