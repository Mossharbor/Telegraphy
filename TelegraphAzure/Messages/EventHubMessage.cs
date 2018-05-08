using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    internal class EventHubMessage : SimpleMessage<Microsoft.Azure.EventHubs.EventData>
    {
        public EventHubMessage(Microsoft.Azure.EventHubs.EventData message) : base(message)
        {
        }
    }
}
