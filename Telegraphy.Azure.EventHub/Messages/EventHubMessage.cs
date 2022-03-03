using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    internal class EventHubMessage : SimpleMessage<global::Azure.Messaging.EventHubs.EventData>
    {
        public EventHubMessage(global::Azure.Messaging.EventHubs.EventData message) : base(message)
        {
        }
    }
}
