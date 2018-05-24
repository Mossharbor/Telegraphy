using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Azure
{
    public class SendBytesToEventHub : IActor
    {
        EventHubDataPublisher eventHubClient;

        public SendBytesToEventHub(string eventHubonnectionString, string eventHubName, bool createEventHubIfItDoesNotExist = true)
        {
            eventHubClient = EventHubPublishOperator<byte[]>.GetEventHubClient(eventHubonnectionString, eventHubName, createEventHubIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage<byte[]>(msg);
            eventHubClient.Send(eventData);
            return true;
        }

        internal static EventData BuildMessage<MsgType>(IActorMessage msg) where MsgType : class
        {
            byte[] msgBytes = TempSerialization.GetBytes<MsgType>(msg);
            return new EventData(msgBytes);
        }
    }
}
