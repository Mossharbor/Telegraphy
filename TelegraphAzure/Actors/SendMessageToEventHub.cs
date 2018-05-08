using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendMessageToEventHub : IActor
    {
        EventHubClient eventHubClient;

        public SendMessageToEventHub(string eventHubonnectionString, string eventHubName, bool createQueueIfItDoesNotExist = true)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubonnectionString)
            {
                EntityPath = eventHubName
            };
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage(msg, MessageSource.EntireIActor);
            eventHubClient.SendAsync(eventData).Wait();
            return true;
        }
    }
}
