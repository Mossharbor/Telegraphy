using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class EventHubStringReceptionOperator : EventHubBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;
        static MessageSource defaultSource = MessageSource.StringMessage;

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }
        
        public EventHubStringReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource)
        {
        }

        public EventHubStringReceptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource)
        {
        }
        
        public EventHubStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd(), createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        public EventHubStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, EventHubActorMessageReceptionOperator.GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, position, createEventHubIfItDoesNotExist), defaultSource, concurrency)
        {
        }

        private EventHubStringReceptionOperator(LocalConcurrencyType type, EventHubDataReciever reciever, MessageSource source, uint concurrency)
           : this(new LocalSwitchboard(type, concurrency), reciever, source)
        { }

        private EventHubStringReceptionOperator(EventHubDataReciever reciever, MessageSource source, uint concurrency)
            : this(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount, concurrency), reciever, source)
        { }

        private EventHubStringReceptionOperator(ILocalSwitchboard switchBoard, EventHubDataReciever reciever, MessageSource source)
            : base(switchBoard, reciever, source)
        { }
    }
}
