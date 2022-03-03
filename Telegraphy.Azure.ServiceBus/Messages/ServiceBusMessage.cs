using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using global::Azure.Messaging.ServiceBus;

namespace Telegraphy.Azure
{
    internal class ServiceBusMessage : SimpleMessage<global::Azure.Messaging.ServiceBus.ServiceBusMessage>
    {
        public ServiceBusMessage(global::Azure.Messaging.ServiceBus.ServiceBusMessage message) : base(message)
        {
        }
    }
}
