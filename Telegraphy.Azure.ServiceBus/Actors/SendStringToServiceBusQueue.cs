using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendStringToServiceBusQueue : DeliverToServiceBusQueue<string>
    {
        public SendStringToServiceBusQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
            : base(storageConnectionString, queueName, createQueueIfItDoesNotExist)
        { }
    }
}
