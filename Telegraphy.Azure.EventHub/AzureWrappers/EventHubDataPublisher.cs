using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class EventHubDataPublisher
    {
        EventHubProducerClient client;
        string connectionString;
        string eventHubName;
        string[] consumerGroups = null;

        public EventHubDataPublisher(string connectionString,string eventHubName)
        {
            this.eventHubName = eventHubName;
            this.connectionString = connectionString;
            client = new EventHubProducerClient(connectionString, eventHubName);
        }
        
        internal void Send(EventData data)
        {
            this.client.SendAsync(new EventData[] { data });
            //using (var eventBatch = client.CreateBatchAsync().Result)
            //{
            //    eventBatch.TryAdd(data);
            //    this.client.SendAsync(eventBatch).Wait();
            //}
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
                    ConsumerGroupDescription sd;
                    if (!ns.ConsumerGroupExists(this.eventHubName, consumerGroup, out sd))
                        ns.CreateConsumerGroup(this.eventHubName, consumerGroup);
                });
            }
        }
    }
}
