﻿using Azure.Messaging.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendMessageToEventHub<MsgType> : IActor
    {
        EventHubDataPublisher eventHubClient;

        public SendMessageToEventHub(string eventHubonnectionString, string eventHubName, bool createEventHubIfItDoesNotExist = true)
        {
            eventHubClient = EventHubPublishOperator<IActorMessage>.GetEventHubClient(eventHubonnectionString, eventHubName, createEventHubIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage<IActorMessage>(msg);
            eventHubClient.Send(eventData);
            return true;
        }
    }
}
