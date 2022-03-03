using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using global::Azure.Messaging.EventHubs;
    using global::Azure.Messaging.EventHubs.Consumer;
    using global::Azure.Messaging.EventHubs.Primitives;
    

    internal class EventHubDataSubscriber 
    {
        EventHubConsumerClient client = null;
        PartitionReceiver reciever = null;
        string connectionString = null;
        string eventHubName, consumerGroup, partition;
        string[] consumerGroups;

        public EventHubDataSubscriber(string connectionstring, string eventHubName)
            : this(connectionstring, eventHubName, EventHubConsumerClient.DefaultConsumerGroupName, "1")
        { }

        public EventHubDataSubscriber(string connectionstring, string eventHubName, string consumerGroup)
            : this(connectionstring, eventHubName, consumerGroup, "1")
        { }

        public EventHubDataSubscriber(string connectionstring, string eventHubName, string consumerGroup, string partitionId)
        {
            this.connectionString = connectionstring;
            client = new EventHubConsumerClient(consumerGroup, connectionString, eventHubName);
            reciever = new PartitionReceiver(
                            consumerGroup,
                            partitionId,
                            EventPosition.Earliest,
                            connectionString,
                            eventHubName);
            this.eventHubName = eventHubName;
            this.consumerGroup = consumerGroup;
            this.consumerGroups = new string[] { consumerGroup };
            this.partition = partitionId;
        }

        private string[] partitionIds = null;
        public string[] GetPartitionIds(string eventHubConnectionString, string evenHubName)
        {
            if (null != partitionIds)
                return partitionIds;
            this.partitionIds = this.client.GetPartitionIdsAsync().Result;

            return partitionIds;
        }

        /// <summary>
        ///   yields a batch of global::Azure.Messaging.EventHubs.EventData from the
        //     partition on which this receiver is created. Returns 'null' if no EventData is
        //     present.
        /// </summary>
        /// <param name="maxMessageCount"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public IEnumerable<EventData> Recieve(int numOfMessagesToDequeue,TimeSpan? waitTime)
        {
            IEnumerable<EventData> data = null;
            if (waitTime.HasValue)
            {
                // TODO some form of this is better if we get no partition id.
                /*var en = client.ReadEventsAsync().GetAsyncEnumerator();
                while (en.MoveNextAsync().Result)
                {
                    var cur = en.Current;

                }*/
                data = reciever.ReceiveBatchAsync(numOfMessagesToDequeue, waitTime.Value).Result;
            }
            else
            {
                data = reciever.ReceiveBatchAsync(numOfMessagesToDequeue).Result;
            }

            return data;
        }

        internal void Close()
        {
            client.CloseAsync().Wait();
        }
        public void CreateIfNotExists()
        {
            throw new NotImplementedException("Nedd to implment create if not exists event hub");

            /*NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
            EventHubDescription qd;
            if (!ns.EventHubExists(this.eventHubName, out qd))
                ns.CreateTopic(this.eventHubName);

            if (null != consumerGroups && consumerGroups.Any())
            {
                Parallel.ForEach(consumerGroups, consumerGroup =>
                {
                    if (consumerGroup.Equals(EventHubConsumerClient.DefaultConsumerGroupName))
                        return;

                    ConsumerGroupDescription sd;
                    if (!ns.ConsumerGroupExists(this.eventHubName, consumerGroup, out sd))
                        ns.CreateConsumerGroup(this.eventHubName, consumerGroup);
                });
            }*/
        }
    }
}
