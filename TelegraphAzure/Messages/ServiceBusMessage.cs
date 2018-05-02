using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    internal class ServiceBusMessage : SimpleMessage<Microsoft.Azure.ServiceBus.Message>
    {
        public ServiceBusMessage(Microsoft.Azure.ServiceBus.Message message) : base(message)
        {
        }
    }
}
