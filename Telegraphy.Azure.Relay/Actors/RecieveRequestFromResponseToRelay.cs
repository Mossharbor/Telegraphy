using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.Azure.Relay;

namespace Telegraphy.Azure.Relay
{
    public class RecieveRequestFromResponseToRelay : IActor
    {
        RelayConnectionStringBuilder connectionStringBuilder;
        HybridConnectionClient client;
        Task<HybridConnectionStream> connectionTask = null;

        public RecieveRequestFromResponseToRelay(string relayConnectionString)
        {
            //https://docs.microsoft.com/en-us/azure/service-bus-relay/relay-hybrid-connections-dotnet-api-overview
            var csb = new RelayConnectionStringBuilder(relayConnectionString);
            client = new HybridConnectionClient(csb.ToString());
            connectionTask = client.CreateConnectionAsync();
        }
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!connectionTask.IsCompleted)
                connectionTask.Wait();

            var hybridConnectionStream = connectionTask.Result;

            byte[] msgBytes = null;
            hybridConnectionStream.Write(msgBytes, 0, msgBytes.Length);

            // TODO get the response
            throw new NotImplementedException();
        }
    }
}
