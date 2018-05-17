using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class EventHubByteArrayReceptionOperator : EventHubBaseOperator
    {
        const int DefaultDequeueMaxCount = EventHubBaseOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = EventHubBaseOperator.DefaultConcurrency;
        static MessageSource defaultSource = MessageSource.ByteArrayMessage;

        public EventHubByteArrayReceptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubByteArrayReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubByteArrayReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubByteArrayReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubByteArrayReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubByteArrayReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        private EventHubByteArrayReceptionOperator(LocalConcurrencyType type, EventHubDataReciever reciever, MessageSource source, uint concurrency)
           : this(new LocalSwitchboard(type, concurrency), reciever, source)
        { }

        private EventHubByteArrayReceptionOperator(EventHubDataReciever reciever, MessageSource source, uint concurrency)
            : this(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount, concurrency), reciever, source)
        { }

        private EventHubByteArrayReceptionOperator(ILocalSwitchboard switchBoard, EventHubDataReciever reciever, MessageSource source)
            : base(switchBoard, reciever, source)
        { }
    }
}
