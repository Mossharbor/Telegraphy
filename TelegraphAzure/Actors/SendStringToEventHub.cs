using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendStringToEventHub : IActor
    {
        EventHubDataDeliverer eventHubClient;

        public SendStringToEventHub(string eventHubonnectionString, string eventHubName, bool createEventHubIfItDoesNotExist = true)
        {
            eventHubClient = EventHubActorMessageDeliveryOperator.GetEventHubClient(eventHubonnectionString, eventHubName, createEventHubIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage(msg, MessageSource.StringMessage);
            eventHubClient.Send(eventData);
            return true;
        }
    }
}
