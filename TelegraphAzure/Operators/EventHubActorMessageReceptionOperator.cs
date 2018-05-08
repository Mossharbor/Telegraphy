using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class EventHubActorMessageReceptionOperator: EventHubBaseOperator
    {
        static MessageSource defaultSource = MessageSource.EntireIActor;

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        internal static EventHubDataReciever GetEventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }

        internal static EventHubDataReciever GetEventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataReciever(connectionstring, eventHubName, consumerGroup, position);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }

        internal static EventHubDataReciever GetEventHubDataReciever(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataReciever(connectionstring, eventHubName, position);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }
    }
}
