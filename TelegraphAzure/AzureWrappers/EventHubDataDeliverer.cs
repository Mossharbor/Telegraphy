using Microsoft.Azure.EventHubs;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class EventHubDataDeliverer
    {
        EventHubClient client;
        string connectionString;
        string eventHubName;
        string[] consumerGroups = null;

        public EventHubDataDeliverer(string connectionString,string eventHubName)
        {
            this.eventHubName = eventHubName;
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString)
            {
                EntityPath = eventHubName
            };

            this.connectionString = connectionString;
            client = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }
        
        internal void Send(EventData data)
        {
            this.client.SendAsync(data).Wait();
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
