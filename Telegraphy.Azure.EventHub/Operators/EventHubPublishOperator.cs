using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    using Telegraphy.Net;

    public class EventHubPublishOperator<T> : EventHubBaseOperator<T> where T: class
    {
        public EventHubPublishOperator(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(GetEventHubClient(connectionString, eventHubName, createEventHubIfItDoesNotExist))
        {
        }

        internal static EventHubDataPublisher GetEventHubClient(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataPublisher(connectionString, eventHubName);

            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();

            return t;
        }
    }
}
