using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.Azure.ServiceBus;

namespace Telegraphy.Azure
{
    internal class ServiceBusMessage : SimpleMessage<Message>
    {
        public ServiceBusMessage(Microsoft.Azure.ServiceBus.Message message) : base(message)
        {
        }
    }
}
