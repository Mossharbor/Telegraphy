using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Azure.Relay
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telegraphy.Net;
    using Mossharbor.AzureWorkArounds.ServiceBus;
    //using Microsoft.Azure.EventHubs.Processor;
    //using Microsoft.Azure.EventHubs;
    //using System.Collections.Concurrent;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.Relay;
    using System.Net;
    using System.IO;

    [TestClass]
    public class AzureRelayTests
    {
        public void DeleteRelay(string relayName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.RelayConnectionString);
            RelayDescription rd;
            if (ns.RelayExists(relayName, out rd))
                ns.DeleteRelay(relayName);
        }

        public void DeleteHybridConnection(string relayName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.RelayConnectionString);
            HybridConnectionDescription rd;
            if (ns.HybridConnectionExists(relayName, out rd))
                ns.DeleteHybridConnection(relayName);
        }

        public void CreateRelay(string relayName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.RelayConnectionString);
            RelayDescription rd;
            if (!ns.RelayExists(relayName, out rd))
                ns.CreateRelay(relayName, RelayType.NetTcp);
        }

        public void CreateHybridConnection(string hybridConnectionName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.RelayConnectionString);
            HybridConnectionDescription rd;
            if (!ns.HybridConnectionExists(hybridConnectionName, out rd))
                ns.CreateHybridConnection(hybridConnectionName);
        }

        public HybridConnectionListener CreateHybridListener(string hybridConnectionName)
        {
            RelayConnectionStringBuilder connectionStringBuilder = new RelayConnectionStringBuilder(Connections.RelayConnectionString) { EntityPath = hybridConnectionName };
            //var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            //var uri = new Uri(string.Format("https://{0}/{1}", connectionStringBuilder.Endpoint.Host, hybridConnectionName));
            var listener = new HybridConnectionListener(connectionStringBuilder.ToString());

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Online"); };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                // Do something with context.Request.Url, HttpMethod, Headers, InputStream...
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.StatusDescription = "OK";
                using (var sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.WriteLine("hello!");
                }

                // The context MUST be closed here
                context.Response.Close();
            };

            listener.OpenAsync().Wait();
            Console.WriteLine("Server listening");
            return listener;
        }

        private static IActor FailOnException(Exception exception, IActor actor, IActorMessage actorMessage, IActorInvocation actorInvocation)
        {
            Assert.Fail();
            return actor;
        }

        [TestMethod]
        public void TestSendingStringToHybridConnection()
        {
            string relayName = "TestHybridConnection";
            CreateHybridConnection(relayName);
            Telegraph.Instance.Register(typeof(Exception), FailOnException);
            HybridConnectionListener listener = null;
            try
            {
                listener = CreateHybridListener(relayName);
                var relayConnection = new Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRelayRequest(Connections.RelayConnectionString, relayName);

                Telegraph.Instance.Register<string, Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRelayRequest>(() => relayConnection);
                bool success = Telegraph.Instance.Ask("Hello").Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(success);
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteRelay(relayName);
            }
        }

        [TestMethod]
        public void TestSendingStringToWcfRelay()
        {
            string relayName = "TestRelay";
            CreateRelay(relayName);
            try
            {
                var relayConnection = new Telegraphy.Azure.Relay.Wcf.RecieveResponseFromRelayRequest(Connections.RelayConnectionString, relayName);

                Telegraph.Instance.Register<string, Telegraphy.Azure.Relay.Wcf.RecieveResponseFromRelayRequest>(() => relayConnection);
                bool success = Telegraph.Instance.Ask("Hello").Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(success);
            }
            finally
            {
                DeleteRelay(relayName);
            }
        }
    }
}
