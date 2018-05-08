using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.EventHubs;
    using Microsoft.Azure.ServiceBus;
    using Mossharbor.AzureWorkArounds.ServiceBus;

    internal class EventHubDataReciever 
    {
        EventHubClient client = null;
        PartitionReceiver reciever = null;
        long lastSequenceNumber = 0;
        string connectionString = null;
        string eventHubName, consumerGroup, partition;
        string[] consumerGroups;

        public EventHubDataReciever(string connectionstring, string eventHubName)
            : this(connectionstring, eventHubName, PartitionReceiver.DefaultConsumerGroupName, "1", EventPosition.FromEnd())
        { }

        public EventHubDataReciever(string connectionstring, string eventHubName, EventPosition position)
            : this(connectionstring, eventHubName, PartitionReceiver.DefaultConsumerGroupName, "1", position)
        { }
        
        public EventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup)
            : this(connectionstring, eventHubName, consumerGroup, "1", EventPosition.FromEnd())
        { }

        public EventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, EventPosition position)
            : this(connectionstring, eventHubName, consumerGroup, "1", position)
        { }

        public EventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, string partitionId)
            : this(connectionstring, eventHubName, consumerGroup, partitionId, EventPosition.FromEnd())
        { }

        public EventHubDataReciever(string connectionstring, string eventHubName, string consumerGroup, string partitionId, EventPosition position)
        {
            this.connectionString = connectionstring;
            string connectionStringWithEntityPath = connectionstring;
            if (!connectionStringWithEntityPath.Contains("EntityPath")) // https://github.com/Azure/azure-webjobs-sdk/commit/be47a3075076c0094a1358dadada01954ec28c79#diff-c255f38efc7c153a9ce53cecefe7b7b0R44
            {
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(connectionstring) { EntityPath = eventHubName };
                connectionStringWithEntityPath = connectionStringBuilder.ToString();
            }

            //"Endpoint=sb://test89123-ns-x.servicebus.windows.net/;SharedAccessKeyName=ReceiveRule;SharedAccessKey=secretkey;EntityPath=path2"

            client = EventHubClient.CreateFromConnectionString(connectionStringWithEntityPath);
            reciever = client.CreateReceiver(consumerGroup, partitionId, position);
            this.eventHubName = eventHubName;
            this.consumerGroup = consumerGroup;
            this.consumerGroups = new string[] { consumerGroup };
            this.partition = partitionId;
        }

        private static string[] partitionIds = null;
        public static string[] GetPartitionIds(string eventHubConnectionString, string evenHubName)
        {
            if (null != partitionIds)
                return partitionIds;
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(eventHubConnectionString);
            EventHubDescription ed = ns.GetEventHub(evenHubName);
            partitionIds = Array.ConvertAll(ed.PartitionIds, item=>item.ToString());
            return partitionIds;
        }

        /// <summary>
        ///   yields a batch of Microsoft.Azure.EventHubs.EventData from the
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
                data = reciever.ReceiveAsync(numOfMessagesToDequeue, waitTime.Value).Result;
            else
                data = reciever.ReceiveAsync(numOfMessagesToDequeue).Result;

            lastSequenceNumber = data.Last().SystemProperties.SequenceNumber;
            return data;
        }

        internal void Close()
        {
            client.Close();
        }

        public long ApproximateCount()
        {
            return client.GetPartitionRuntimeInformationAsync(this.partition).Result.LastEnqueuedSequenceNumber - lastSequenceNumber;
        }


        public void CreateIfNotExists()
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
            EventHubDescription qd;
            if (!ns.EventHubExists(this.eventHubName, out qd))
                ns.CreateTopic(this.eventHubName);

            if (null != consumerGroups && consumerGroups.Any())
            {
                Parallel.ForEach(consumerGroups, consumerGroup =>
                {
                    if (consumerGroup.Equals(PartitionReceiver.DefaultConsumerGroupName))
                        return;

                    ConsumerGroupDescription sd;
                    if (!ns.ConsumerGroupExists(this.eventHubName, consumerGroup, out sd))
                        ns.CreateConsumerGroup(this.eventHubName, consumerGroup);
                });
            }
        }
    }
}
