using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToEventHub : IActor
    {
        EventHubClient eventHubClient;

        public SendBytesToEventHub(string eventHubonnectionString, string eventHubName, bool createQueueIfItDoesNotExist = true)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubonnectionString)
            {
                EntityPath = eventHubName
            };
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            EventData eventData = SendBytesToEventHub.BuildMessage(msg, MessageSource.ByteArrayMessage);
            eventHubClient.SendAsync(eventData).Wait();
            return true;
        }

        internal static EventData BuildMessage<T>(T msg, MessageSource messageSource) where T : class, IActorMessage
        {
            byte[] msgBytes = null;
            switch (messageSource)
            {
                case MessageSource.EntireIActor:
                    {
                        var serializeTask = Telegraph.Instance.Ask(new SerializeMessage(msg));
                        msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
                    }
                    break;

                case MessageSource.ByteArrayMessage:
                    if ((msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                        msgBytes = (byte[])(msg as IActorMessage).Message;
                    else
                        throw new NotConfiguredToSerializeThisTypeOfMessageException("Byte[]");
                    break;
                case MessageSource.StringMessage:
                    if ((msg as IActorMessage).Message.GetType().Name.Equals("String"))
                        msgBytes = Encoding.UTF8.GetBytes((string)(msg as IActorMessage).Message);
                    else
                        throw new NotConfiguredToSerializeThisTypeOfMessageException("String");
                    break;
                default:
                    throw new NotImplementedException(messageSource.ToString());
            }
            return new EventData(msgBytes);
        }
    }
}
