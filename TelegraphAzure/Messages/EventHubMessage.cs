using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    internal class EventHubMessage : SimpleMessage<EventData>
    {
        public EventHubMessage(EventData message) : base(message)
        {
        }
    }
}
