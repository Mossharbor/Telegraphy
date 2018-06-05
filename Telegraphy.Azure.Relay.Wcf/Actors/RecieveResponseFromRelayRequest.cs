using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Relay;
using Telegraphy.Net;

namespace Telegraphy.Azure.Relay.Wcf
{
    public class RecieveResponseFromRelayRequest : IActor
    {
        string relayName;
        RelayConnectionStringBuilder connectionStringBuilder;
        HybridConnectionClient client;
        Task<HybridConnectionStream> connectionTask = null;

        public RecieveResponseFromRelayRequest(string relayConnectionString, string relayName)
        {
            //https://docs.microsoft.com/en-us/azure/service-bus-relay/relay-hybrid-connections-dotnet-api-overview
            connectionStringBuilder = new RelayConnectionStringBuilder(relayConnectionString) { EntityPath = relayName };
            client = new HybridConnectionClient(connectionStringBuilder.ToString());
            connectionTask = client.CreateConnectionAsync();
            this.relayName = relayName;
        }
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!connectionTask.IsCompleted)
                connectionTask.Wait();

            var hybridConnectionStream = connectionTask.Result;

            byte[] msgBytes = null;
            hybridConnectionStream.Write(msgBytes, 0, msgBytes.Length);

            // TODO get the response
            //throw new NotImplementedException();
            return true;
        }
    }
}
