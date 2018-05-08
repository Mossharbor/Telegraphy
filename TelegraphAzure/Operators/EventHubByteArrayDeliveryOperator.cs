using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class EventHubByteArrayDeliveryOperator : EventHubBaseOperator
    {
        public EventHubByteArrayDeliveryOperator(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(GetEventHubClient(connectionString, eventHubName, createEventHubIfItDoesNotExist), MessageSource.ByteArrayMessage)
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
