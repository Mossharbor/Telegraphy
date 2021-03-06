﻿using Microsoft.Azure.Relay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure.Relay.Hybrid
{
    public class RecieveResponseFromRequestByType : IActor
    {
        string hybridConnectionName;
        Type requestType, responseType;
        RelayConnectionStringBuilder connectionStringBuilder;
        //HybridConnectionClient client;
        //Task<HybridConnectionStream> connectionTask = null;

        internal RecieveResponseFromRequestByType(Type requestType, Type responseType, string relayConnectionString)
        {
            connectionStringBuilder = new RelayConnectionStringBuilder(relayConnectionString) { EntityPath = hybridConnectionName };
            this.hybridConnectionName = connectionStringBuilder.EntityPath;
            this.requestType = requestType;
            this.responseType = responseType;
        }
        internal RecieveResponseFromRequestByType(Type requestType, Type responseType, string relayConnectionString, string hybridConnectionName)
        {
            //https://docs.microsoft.com/en-us/azure/service-bus-relay/relay-hybrid-connections-dotnet-api-overview
            connectionStringBuilder = new RelayConnectionStringBuilder(relayConnectionString) { EntityPath = hybridConnectionName };
            //client = new HybridConnectionClient(connectionStringBuilder.ToString());
            //connectionTask = client.CreateConnectionAsync();
            this.hybridConnectionName = hybridConnectionName;
            this.requestType = requestType;
            this.responseType = responseType;
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
            HttpContent content = null;
            if (requestType == typeof(string))
                content = new StringContent(msg.Message as string);
            else if (requestType == typeof(byte[]))
                content = new ByteArrayContent(msg.Message as byte[]);
            else
            {
                MethodInfo method = typeof(TempSerialization).GetMethod("GetBytes");
                MethodInfo generic = method.MakeGenericMethod(requestType);
                content = new ByteArrayContent((byte[])generic.Invoke(null, new object[] { msg }));
            }

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
            if (null != msg.Status)
            {
                if (null != response.Content)
                {
                    if (responseType == typeof(string))
                        msg.Status.SetResult(response.Content.ReadAsStringAsync().Result.ToActorMessage());
                    else if (responseType == typeof(byte[]))
                        msg.Status.SetResult(response.Content.ReadAsByteArrayAsync().Result.ToActorMessage());
                    else
                    {
                        MethodInfo method = typeof(TempSerialization).GetMethod("GetTypeFromBytes");
                        MethodInfo generic = method.MakeGenericMethod(responseType);
                        IActorMessage responseMsg = (IActorMessage)generic.Invoke(null, new object[] { response.Content.ReadAsByteArrayAsync().Result });
                        msg.Status.SetResult(responseMsg);
                    }
                }
                else
                {
                    msg.Status.SetResult(msg);
                }
            }
            return true;
        }
    }
}
