using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class EventHubReceptionOperator<T> : EventHubBaseOperator<T> where T:class
    {
        public EventHubReceptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist))
        {
        }

        public EventHubReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist))
        {
        }

        public EventHubReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist))
        {
        }

        public EventHubReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist))
        {
        }

        public EventHubReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist))
        {
        }

        public EventHubReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist))
        {
        }

        public EventHubReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        private EventHubReceptionOperator(LocalConcurrencyType type, EventHubDataSubscriber reciever, uint concurrency)
           : this(new LocalSwitchboard(type, concurrency), reciever)
        { }

        private EventHubReceptionOperator(EventHubDataSubscriber reciever, uint concurrency)
            : this(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount, concurrency), reciever)
        { }

        private EventHubReceptionOperator(ILocalSwitchboard switchBoard, EventHubDataSubscriber reciever)
            : base(switchBoard, reciever)
        { }


        internal static EventHubDataSubscriber GetEventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataSubscriber(connectionstring, eventHubName, consumerGroup, partitionId, position);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }

        internal static EventHubDataSubscriber GetEventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataSubscriber(connectionstring, eventHubName, consumerGroup, position);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }

        internal static EventHubDataSubscriber GetEventHubDataReciever(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataSubscriber(connectionstring, eventHubName, position);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }
    }
}
