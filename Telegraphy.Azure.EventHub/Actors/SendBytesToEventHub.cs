using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Azure
{
    public class SendBytesToEventHub : IActor
    {
        EventHubDataDeliverer eventHubClient;

        public SendBytesToEventHub(string eventHubonnectionString, string eventHubName, bool createEventHubIfItDoesNotExist = true)
        {
            eventHubClient = EventHubDeliveryOperator<byte[]>.GetEventHubClient(eventHubonnectionString, eventHubName, createEventHubIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage<byte[]>(msg);
            eventHubClient.Send(eventData);
            return true;
        }

        internal static EventData BuildMessage<MsgType>(IActorMessage msg) where MsgType : class
        {
            byte[] msgBytes = null;
            if (typeof(MsgType) == typeof(string))
            {
                if ((msg as IActorMessage).Message.GetType().Name.Equals("String"))
                    msgBytes = Encoding.UTF8.GetBytes((string)(msg as IActorMessage).Message);
                else
                    throw new NotConfiguredToSerializeThisTypeOfMessageException("String");
            }
            else if (typeof(MsgType) == typeof(byte[]))
            {
                if ((msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                    msgBytes = (byte[])(msg as IActorMessage).Message;
                else
                    throw new NotConfiguredToSerializeThisTypeOfMessageException("Byte[]");
            }
            else
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeIActorMessage(msg));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }

            return new EventData(msgBytes);
        }
    }
}
