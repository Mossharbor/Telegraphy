﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.Azure.Relay;
using System.Net.Http;

namespace Telegraphy.Azure.Relay.Hybrid
{
    public class RecieveResponseFromRelayRequest<MsgType> : IActor where MsgType:class
    {
        string hybridConnectionName;

        RelayConnectionStringBuilder connectionStringBuilder;
        //HybridConnectionClient client;
        //Task<HybridConnectionStream> connectionTask = null;

        public RecieveResponseFromRelayRequest(string relayConnectionString, string hybridConnectionName)
        {
            //https://docs.microsoft.com/en-us/azure/service-bus-relay/relay-hybrid-connections-dotnet-api-overview
            connectionStringBuilder = new RelayConnectionStringBuilder(relayConnectionString) { EntityPath = hybridConnectionName };
            //client = new HybridConnectionClient(connectionStringBuilder.ToString());
            //connectionTask = client.CreateConnectionAsync();
            this.hybridConnectionName = hybridConnectionName;
        }
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            //if (!connectionTask.IsCompleted)
            //    connectionTask.Wait();

            //var hybridConnectionStream = connectionTask.Result;

            //byte[] msgBytes = null;
            //hybridConnectionStream.Write(msgBytes, 0, msgBytes.Length);

            // TODO get the response
            //throw new NotImplementedException();

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            //var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("testKey", "1");
            var uri = new Uri(string.Format("https://{0}/{1}", connectionStringBuilder.Endpoint.Host, hybridConnectionName));
            var token = (tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1)).Result).TokenString;
            HttpContent content = (msg.GetType() == typeof(string)) ? new StringContent(msg.Message as string) : new ByteArrayContent(TempSerialization.GetBytes<MsgType>(msg));
                
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = content
            };
            request.Headers.Add("ServiceBusAuthorization", token);
            var response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            if (null != msg.Status && null != response.Content)
            {
                if (typeof(MsgType) == typeof(string))
                    msg.Status.SetResult(response.Content.ReadAsStringAsync().Result.ToActorMessage());
                else if (typeof(MsgType) == typeof(byte[]))
                    msg.Status.SetResult(response.Content.ReadAsByteArrayAsync().Result.ToActorMessage());
                else
                {
                    byte[] msgBytes = response.Content.ReadAsByteArrayAsync().Result;
                    var t = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(msgBytes));
                    msg.Status.SetResult(t.Result as IActorMessage);
                }
            }
            return true;
        }
    }
}