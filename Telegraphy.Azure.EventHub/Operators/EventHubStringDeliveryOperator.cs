using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class EventHubStringDeliveryOperator : EventHubBaseOperator
    {
        public EventHubStringDeliveryOperator(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(GetEventHubClient(connectionString, eventHubName, createEventHubIfItDoesNotExist), Telegraphy.Net.MessageSource.StringMessage)
        {
        }

        internal static EventHubDataDeliverer GetEventHubClient(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataDeliverer(connectionString, eventHubName);

            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();

            return t;
        }
    }
}
