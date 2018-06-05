using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Azure.Relay
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telegraphy.Net;
    using Telegraphy.Azure.Relay;
    using Telegraphy.Azure;
    using Mossharbor.AzureWorkArounds.ServiceBus;
    using Microsoft.Azure.EventHubs.Processor;
    using Microsoft.Azure.EventHubs;
    using System.Collections.Concurrent;

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

        public void CreateRelay(string relayName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.RelayConnectionString);
            RelayDescription rd;
            if (!ns.RelayExists(relayName, out rd))
                ns.CreateRelay(relayName, RelayType.NetTcp);
        }

        [TestMethod]
        public void TestSendingStringToRelay()
        {
            string relayName = "TestRelay";
            CreateRelay(relayName);
            try
            {
                RecieveResponseFromRelayRequest relayConnection = new RecieveResponseFromRelayRequest(Connections.RelayConnectionString, relayName);

                Telegraph.Instance.Register<string, RecieveResponseFromRelayRequest>(() => relayConnection);
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
