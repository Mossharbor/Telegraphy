using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    using Telegraphy.Net;

    public class EventHubDeliveryOperator<T> : EventHubBaseOperator<T> where T: class
    {
        public EventHubDeliveryOperator(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(GetEventHubClient(connectionString, eventHubName, createEventHubIfItDoesNotExist))
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
