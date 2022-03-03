using Azure.Messaging.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class EventHubSubscriptionOperator<T> : EventHubBaseOperator<T> where T:class
    {
        public EventHubSubscriptionOperator(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubSubscriptionOperator(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubSubscriptionOperator(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubSubscriptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, createEventHubIfItDoesNotExist))
        {
        }

        public EventHubSubscriptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, createEventHubIfItDoesNotExist))
        {
        }

        public EventHubSubscriptionOperator(ILocalSwitchboard switchboard, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false)
            : this(switchboard, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, createEventHubIfItDoesNotExist))
        {
        }

        public EventHubSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        public EventHubSubscriptionOperator(LocalConcurrencyType concurrencyType, string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist = false, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, GetEventHubDataReciever(connectionstring, eventHubName, consumerGroup, partitionId, createEventHubIfItDoesNotExist), concurrency)
        {
        }

        private EventHubSubscriptionOperator(LocalConcurrencyType type, EventHubDataSubscriber reciever, uint concurrency)
           : this(new LocalSwitchboard(type, concurrency), reciever)
        { }

        private EventHubSubscriptionOperator(EventHubDataSubscriber reciever, uint concurrency)
            : this(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount, concurrency), reciever)
        { }

        private EventHubSubscriptionOperator(ILocalSwitchboard switchBoard, EventHubDataSubscriber reciever)
            : base(switchBoard, reciever)
        { }


        internal static EventHubDataSubscriber GetEventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, string partitionId, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataSubscriber(connectionstring, eventHubName, consumerGroup, partitionId);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }

        internal static EventHubDataSubscriber GetEventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataSubscriber(connectionstring, eventHubName, consumerGroup);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }

        internal static EventHubDataSubscriber GetEventHubDataReciever(string connectionstring, string eventHubName, bool createEventHubIfItDoesNotExist)
        {
            var t = new EventHubDataSubscriber(connectionstring, eventHubName);
            if (createEventHubIfItDoesNotExist)
                t.CreateIfNotExists();
            return t;
        }
    }
}
