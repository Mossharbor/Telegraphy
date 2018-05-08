using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace Telegraphy.Azure
{
    public class EventHubActorMessageDeliveryOperator : EventHubBaseOperator
    {
        public EventHubActorMessageDeliveryOperator(string connectionString, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(GetEventHubClient(connectionString, eventHubName, createEventHubIfItDoesNotExist), MessageSource.EntireIActor)
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
