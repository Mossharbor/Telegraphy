using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class EventHubActorMessageReceptionOperator: EventHubBaseOperator
    {
        const int DefaultDequeueMaxCount = EventHubBaseOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = EventHubBaseOperator.DefaultConcurrency;
        static MessageSource defaultSource = MessageSource.EntireIActor;

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        private EventHubActorMessageReceptionOperator(LocalConcurrencyType type, EventHubDataReciever reciever, MessageSource source, uint concurrency)
            : this(new LocalSwitchboard(type, concurrency), reciever, source)
        { }
        
        private EventHubActorMessageReceptionOperator(EventHubDataReciever reciever, MessageSource source, uint concurrency)
            : this(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount, concurrency), reciever, source)
        { }

        private EventHubActorMessageReceptionOperator(ILocalSwitchboard switchBoard, EventHubDataReciever reciever, MessageSource source)
            : base(switchBoard, reciever, source)
        { }

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
