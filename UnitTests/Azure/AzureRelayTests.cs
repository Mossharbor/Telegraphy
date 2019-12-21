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

        public HybridConnectionListener CreateHybridListener(string hybridConnectionName, string responseMessage)
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
                    sw.Write(responseMessage);
                }

                // The context MUST be closed here
                context.Response.Close();
            };

            listener.OpenAsync().Wait();
            Console.WriteLine("Server listening");
            return listener;
        }

        public HybridConnectionListener CreateHybridListener(string hybridConnectionName, byte[] responseBytes)
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
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

                // The context MUST be closed here
                context.Response.Close();
            };

            listener.OpenAsync().Wait();
            Console.WriteLine("Server listening");
            return listener;
        }

        public HybridConnectionListener CreateHybridListener(string hybridConnectionName, PingPong.Pong response)
        {
            RelayConnectionStringBuilder connectionStringBuilder = new RelayConnectionStringBuilder(Connections.RelayConnectionString) { EntityPath = hybridConnectionName };
            //var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            //var uri = new Uri(string.Format("https://{0}/{1}", connectionStringBuilder.Endpoint.Host, hybridConnectionName));
            var listener = new HybridConnectionListener(connectionStringBuilder.ToString());

            SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(response);
            IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
            serializer.OnMessageRecieved(sMsg);
            byte[] responseBytes = (byte[])sMsg.ProcessingResult;

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
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

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
            string relayName = "TestSendingStringToHybridConnection";
            string responseMessage = "Well hello to you!!";
            CreateHybridConnection(relayName);
            Telegraph.Instance.Register(typeof(Exception), FailOnException);
            HybridConnectionListener listener = null;
            try
            {
                listener = CreateHybridListener(relayName, responseMessage);
                var relayConnection = new Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<string,string>(Connections.RelayConnectionString, relayName);

                Telegraph.Instance.Register<string, Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<string, string>>(() => relayConnection);
                var result = Telegraph.Instance.Ask("Hello");
                bool success = result.Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(success);

                if (success)
                    Assert.IsTrue(result.Result.Message.Equals(responseMessage));
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteRelay(relayName);
            }
        }

        [TestMethod]
        public void TestSendingActorMessageToHybridConnection()
        {
            string relayName = "TestSendingActorMessageToHybridConnection";
            string sendMessage = "Hello";
            var actorSendMessage = new PingPong.Ping(sendMessage);
            var actorResponseMessage = new PingPong.Pong();
            CreateHybridConnection(relayName);
            Telegraph.Instance.Register(typeof(Exception), FailOnException);
            HybridConnectionListener listener = null;
            try
            {
                listener = CreateHybridListener(relayName, actorResponseMessage);
                var relayConnection = new Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<PingPong.Ping, PingPong.Pong>(Connections.RelayConnectionString, relayName);

                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Pong>((object msg) => (PingPong.Pong)msg);

                Telegraph.Instance.Register<SerializeMessage<PingPong.Ping>, IActorMessageSerializationActor>(() => serializer);
                Telegraph.Instance.Register<DeserializeMessage<PingPong.Pong>, IActorMessageDeserializationActor>(() => deserializer);

                Telegraph.Instance.Register<PingPong.Ping, Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<PingPong.Ping, PingPong.Pong>>(() => relayConnection);
                var resultTask = Telegraph.Instance.Ask(actorSendMessage);
                bool success = resultTask.Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(success);

                if (success)
                    Assert.IsTrue(resultTask.Result is PingPong.Pong);
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteRelay(relayName);
            }
        }

        [TestMethod]
        public void TestSendingBytesoHybridConnection()
        {
            string relayName = "TestSendingBytesoHybridConnection";
            string responseMessage = "Well hello to you!!";
            string askMessage = "Hello";
            byte[] msgBytes = Encoding.UTF8.GetBytes(askMessage);
            CreateHybridConnection(relayName);
            Telegraph.Instance.Register(typeof(Exception), FailOnException);
            HybridConnectionListener listener = null;
            try
            {
                listener = CreateHybridListener(relayName, responseMessage);
                var relayConnection = new Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<byte[], byte[]>(Connections.RelayConnectionString, relayName);

                Telegraph.Instance.Register<byte[], Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<byte[], byte[]>>(() => relayConnection);
                var result = Telegraph.Instance.Ask(msgBytes);
                bool success = result.Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(success);

                if (success)
                    Assert.IsTrue(Encoding.UTF8.GetString(result.Result.Message as byte[]).Equals(responseMessage));
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteRelay(relayName);
            }
        }

        [TestMethod]
        public void TestSendingBytesToHybridConnectionSwitchboard()
        {
            string connectionName = "TestSendingStringToHybridConnectionSwitchboard";
            string responseMessage = "Well hello to you!!";
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
            CreateHybridConnection(connectionName);
            HybridConnectionListener listener = null;
            try
            {
                listener = CreateHybridListener(connectionName, responseBytes);
                ILocalSwitchboard switchBoard = new Telegraphy.Azure.Relay.Hybrid.HybridConnectionSwitchboard(3, Connections.RelayConnectionString, connectionName);
                IOperator localOP = new LocalQueueOperator(switchBoard);
                Telegraph.Instance.Register(localOP);
                Telegraph.Instance.Register(typeof(Exception), FailOnException);

                // We want to send the byte[] to the localOP which will forward calls to the hybrid connection switchboard operator
                Telegraph.Instance.Register<byte[]>(localOP);

                byte[] resultBytes= (byte[])Telegraph.Instance.Ask(Encoding.UTF8.GetBytes("Foo").ToActorMessage()).Result.Message;
                string responseString = Encoding.UTF8.GetString(resultBytes);
                Assert.IsTrue(responseString.Equals(responseMessage));
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteHybridConnection(connectionName);
            }
        }

        [TestMethod]
        public void TestSendingStringToHybridConnectionSwitchboard()
        {
            string connectionName = "TestSendingStringToHybridConnectionSwitchboard";
            string responseMessage = "Well hello to you!!";
            CreateHybridConnection(connectionName);
            HybridConnectionListener listener = null;
            try
            {
                listener = CreateHybridListener(connectionName, responseMessage);
                ILocalSwitchboard switchBoard = new Telegraphy.Azure.Relay.Hybrid.HybridConnectionSwitchboard(3, Connections.RelayConnectionString, connectionName);
                IOperator localOP = new LocalQueueOperator(switchBoard);
                Telegraph.Instance.Register(localOP);
                Telegraph.Instance.Register(typeof(Exception), FailOnException);

                // We want to send the byte[] to the localOP which will forward calls to the hybrid connection switchboard operator
                Telegraph.Instance.Register<string>(localOP);

                string responseString = (string)Telegraph.Instance.Ask("Foo").Result.Message;
                Assert.IsTrue(responseString.Equals(responseMessage));
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteHybridConnection(connectionName);
            }
        }

        [TestMethod]
        public void TestSubscribingToHybridConnection()
        {
            string connectionName = "TestSubscribingToHybridConnection";
            string responseMessage = "Well hello to you!!";
            CreateHybridConnection(connectionName);
            HybridConnectionListener listener = null;
            try
            {
                //listener = CreateHybridListener(connectionName, responseMessage);
                IOperator localOP = new Telegraphy.Azure.Relay.Hybrid.HybridConnectionSubscriptionOperator<string>(Connections.RelayConnectionString, connectionName);

                bool recieved = false;
                Telegraph.Instance.Register<string>(localOP, (msgString) =>
                {
                    recieved = true;
                    Console.WriteLine(msgString);
                    Assert.IsTrue(responseMessage.Equals(responseMessage));
                });
                Telegraph.Instance.Register(typeof(Exception), FailOnException);

                var client = new Telegraphy.Azure.Relay.Hybrid.RecieveResponseFromRequest<string,string>(Connections.RelayConnectionString, connectionName);
                (client as IActor).OnMessageRecieved(responseMessage.ToActorMessage());

                System.Threading.Thread.Sleep(3000);
                Assert.IsTrue(recieved);
            }
            finally
            {
                try { listener?.CloseAsync().Wait(); } catch (Exception) { }
                DeleteHybridConnection(connectionName);
            }
        }

        [TestMethod]
        public void TestSendingStringToWcfRelay()
        {
            string relayName = "TestRelay";
            CreateRelay(relayName);
            try
            {
                var relayConnection = new Telegraphy.Azure.Relay.Wcf.RecieveResponseFromRequest(Connections.RelayConnectionString, relayName);

                Telegraph.Instance.Register<string, Telegraphy.Azure.Relay.Wcf.RecieveResponseFromRequest>(() => relayConnection);
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
