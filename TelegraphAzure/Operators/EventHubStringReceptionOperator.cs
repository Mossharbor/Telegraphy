using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class EventHubStringReceptionOperator : EventHubBaseOperator
    {
        static MessageSource defaultSource = MessageSource.StringMessage;

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : base(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }
    }
}
