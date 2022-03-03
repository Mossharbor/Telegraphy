using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Azure.ServiceBus
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telegraphy.Net;
    using Telegraphy.Azure;
    using global::Azure.Messaging.ServiceBus;
    
    using global::Azure.Messaging.EventHubs;
    using System.Threading;
    using System.Collections.Concurrent;
    
    using System.IO;
    using global::Azure.Messaging.ServiceBus.Administration;

    [TestClass]
    public class ServiceBusTests
    {
        #region ServiceBus Helpers
        ConcurrentQueue<ServiceBusReceivedMessage> sbMsgQueue = new ConcurrentQueue<ServiceBusReceivedMessage>();

        private ServiceBusProcessor StartProcessing(string queueName)
        {
            CreateServiceBusQueue(queueName);
            ServiceBusClient client = new ServiceBusClient(Connections.ServiceBusConnectionString);
            var processor = client.CreateProcessor(queueName);
            processor.ProcessMessageAsync += (ProcessMessageEventArgs e) =>
            {
                Console.WriteLine("Recieved message");
                sbMsgQueue.Enqueue(e.Message);
                return Task.CompletedTask;
            };
            processor.ProcessErrorAsync += (ProcessErrorEventArgs e) =>
            {
                Console.Error.WriteLine(e.Exception.Message);
                Assert.Fail();
                return Task.CompletedTask;
            };

            processor.StartProcessingAsync().Wait();
            return processor;
        }

        private ServiceBusProcessor StartProcessing(string topicName, string subscription)
        {
            CreateTopicAndSubscriptions(topicName, new string[] { subscription });
            ServiceBusClient client = new ServiceBusClient(Connections.ServiceBusConnectionString);
            var processor = client.CreateProcessor(topicName, subscription);
            processor.ProcessMessageAsync += (ProcessMessageEventArgs e) =>
            {
                Console.WriteLine("Recieved message");
                sbMsgQueue.Enqueue(e.Message);
                return Task.CompletedTask;
            };
            processor.ProcessErrorAsync += (ProcessErrorEventArgs e) =>
            {
                Console.Error.WriteLine(e.Exception.Message);
                Assert.Fail();
                return Task.CompletedTask;
            };

            processor.StartProcessingAsync().Wait();
            return processor;
        }

        private ServiceBusSender GetServiceBusTopicSender(string topicName)
        {
            var busAdmin = new ServiceBusAdministrationClient(Connections.ServiceBusConnectionString);
            if (!busAdmin.TopicExistsAsync(topicName).Result.Value)
                busAdmin.CreateTopicAsync(topicName).Wait();

            ServiceBusClient client = new ServiceBusClient(Connections.ServiceBusConnectionString);
            return client.CreateSender(topicName);
        }

        private ServiceBusReceiver GetServiceBusTopicReciever(string topicName, string subscription)
        {
            CreateTopicAndSubscriptions(topicName, new string[] { subscription });
            ServiceBusClient client = new ServiceBusClient(Connections.ServiceBusConnectionString);
            return client.CreateReceiver(topicName, subscription);
        }

        private ServiceBusSender GetServiceBusQueueSender(string QueueName)
        {
            CreateServiceBusQueue(QueueName);
            ServiceBusClient client = new ServiceBusClient(Connections.ServiceBusConnectionString);
            return client.CreateSender(QueueName);
        }

        private void CreateServiceBusQueue(string serviceBusQueueName)
        {
            var busAdmin = new ServiceBusAdministrationClient(Connections.ServiceBusConnectionString);
            if (!busAdmin.QueueExistsAsync(serviceBusQueueName).Result.Value)
                busAdmin.CreateQueueAsync(serviceBusQueueName).Wait();
        }

        private void CreateTopicAndSubscriptions(string topicName, string[] subscriptions)
        {
            var busAdmin = new ServiceBusAdministrationClient(Connections.ServiceBusConnectionString);

            if (!busAdmin.TopicExistsAsync(topicName).Result.Value)
            {
                busAdmin.CreateTopicAsync(topicName).Wait();
            }

            foreach (var subscription in subscriptions)
            {
                if (!busAdmin.SubscriptionExistsAsync(topicName, subscription).Result.Value)
                {
                    busAdmin.CreateSubscriptionAsync(topicName, subscription).Wait();
                }
            }
        }

        private void CreateSubscriptions(string topicName, string[] subscriptions)
        {
            var busAdmin = new ServiceBusAdministrationClient(Connections.ServiceBusConnectionString);

            foreach (var subscription in subscriptions)
            {
                if (!busAdmin.SubscriptionExistsAsync(topicName, subscription).Result.Value)
                {
                    busAdmin.CreateSubscriptionAsync(topicName, subscription).Wait();
                }
            }
        }

        private void DeleteServiceBusQueue(string serviceBusQueueName)
        {
            var busAdmin = new ServiceBusAdministrationClient(Connections.ServiceBusConnectionString);
            busAdmin.DeleteQueueAsync(serviceBusQueueName).Wait();
        }

        private void DeleteServiceBusTopic(string topic)
        {
            var busAdmin = new ServiceBusAdministrationClient(Connections.ServiceBusConnectionString);
            busAdmin.DeleteTopicAsync(topic).Wait();
        }

        private Task RecieveMessages(ServiceBusReceivedMessage sbMessage, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                sbMsgQueue.Enqueue(sbMessage);
            });
        }
        
        private void WaitForQueue<T>(ConcurrentQueue<T> queue, out T data) where T : class
        {
            int attempts = 0;
            data = default(T);
            while (!queue.TryDequeue(out data) && attempts < 60)
            {
                ++attempts;
                System.Threading.Thread.Sleep(1000);
            }
            if (attempts >= 60)
                Assert.Fail("We took way too long to get data back from the queue");
        }

        #endregion

        #region Service Bus Queue
        [TestMethod]
        public void SendActorMessageToServiceBusQueue()
        {
            string queueName = "test-" + "SendActorMessageToServiceBusQueue".ToLower();
            var proc = this.StartProcessing(queueName);
            try
            {
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToServiceBusQueue<PingPong.Ping>>(() => new SendMessageToServiceBusQueue<PingPong.Ping>(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body.ToArray());
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void SendActorMessageToServiceBusQueueViaOperator()
        {
            string queueName = "test-" + "SendActorMessageToServiceBusQueueViaOperator".ToLower();
            var proc = this.StartProcessing(queueName);
            try
            {
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusQueuePublishOperator<IActorMessage>(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body.ToArray());
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromServiceBusQueue()
        {
            string queueName = "test-" + "RecieveActorMessageFromServiceBusQueue".ToLower();
            try
            {
                this.StartProcessing(queueName);
                var sender = GetServiceBusQueueSender(queueName);
                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                sender.SendMessageAsync(new ServiceBusMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusQueueSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, queueName, false, 2),
                    (PingPong.Ping foo) => { Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void SendStringToServiceBusQueue()
        {
            string queueName = "test-" + "SendStringServiceBusQueue".ToLower();
            var proc = this.StartProcessing(queueName);
            try
            {
                this.StartProcessing(queueName);
                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToServiceBusQueue>(() => new SendStringToServiceBusQueue(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Ask(message).Wait();
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body.ToArray()).Equals(message, StringComparison.CurrentCulture));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void SendActorStringToServiceBusQueueViaOperator()
        {
            string queueName = "test-" + "SendActorStringToServiceBusQueueViaOperator".ToLower();
            var proc = this.StartProcessing(queueName);
            try
            {
                string message = "HelloWorld";

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusQueuePublishOperator<string>(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(message).Wait();

                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                var retMsgs = Encoding.UTF8.GetString(sbMessage.Body.ToArray());
                Assert.IsTrue((retMsgs).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void RecieveStringFromServiceBusQueue()
        {
            string queueName = "test-" + "RecieveStringServiceBusQueue".ToLower();
            try
            {
                var sender = GetServiceBusQueueSender(queueName);
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                sender.SendMessageAsync(new ServiceBusMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusQueueSubscriptionOperator<string>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, queueName, false, 2),
                    (string foo) => { Assert.IsTrue(message.Equals(foo, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void SendBytesToServiceBusQueue()
        {
            string queueName = "test-" + "SendBytesToServiceBusQueue".ToLower();
            var proc = this.StartProcessing(queueName);
            try
            {
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToServiceBusQueue>(() => new SendBytesToServiceBusQueue(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body.ToArray()).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void RecieveBytesFromServiceBusQueue()
        {
            string queueName = "test-" + "RecieveBytesFromServiceBusQueue".ToLower();
            try
            {
                var sender = GetServiceBusQueueSender(queueName);
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                sender.SendMessageAsync(new ServiceBusMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusQueueSubscriptionOperator<byte[]>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, queueName, false, 2),
                    (ValueTypeMessage<byte> foo) => { Assert.IsTrue(message.Equals(Encoding.UTF8.GetString((byte[])foo.Message), StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }
        #endregion

        #region Service Bus Topic
        [TestMethod]
        public void SendActorMessageToServiceBusTopic()
        {
            string TopicName = "test-" + "SendActorMessageToServiceBusTopic".ToLower();

            // we cannot send messages to topics that have no subscriptions in them
            var proc = this.StartProcessing(TopicName, "test");
            try
            {
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToServiceBusTopic<PingPong.Ping>>(() => new SendMessageToServiceBusTopic<PingPong.Ping>(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);
                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body.ToArray());
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void SendActorMessageToServiceBusTopicViaOperator()
        {
            string TopicName = "test-" + "SendActorMessageToServiceBusTopicViaOperator".ToLower();
            // we cannot send messages to topics that have no subscriptions in them
            var proc = this.StartProcessing(TopicName, "test");
            try
            {
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicPublishOperator<IActorMessage>(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body.ToArray());
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromServiceBusTopic()
        {
            string subscription = "firstsubscription";
            string TopicName = "test-" + "RecieveActorMessageFromServiceBusTopic".ToLower();
            var Topic = GetServiceBusTopicSender(TopicName);
            try
            {
                CreateTopicAndSubscriptions(TopicName, new string[] { subscription });

                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                Topic.SendMessageAsync(new ServiceBusMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, TopicName, subscription, false),
                    (PingPong.Ping foo) => { Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void SendStringToServiceBusTopic()
        {
            string TopicName = "test-" + "SendStringServiceBusTopic".ToLower();
            // we cannot send messages to topics that have no subscriptions in them
            var proc = this.StartProcessing(TopicName, "test");
            try
            {
                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToServiceBusTopic>(() => new SendStringToServiceBusTopic(Connections.ServiceBusConnectionString, TopicName, true));

                for (int i = 0; i < 100; ++i)
                {
                    Telegraph.Instance.Ask(message).Wait();
                    ServiceBusReceivedMessage sbMessage = null;
                    WaitForQueue(sbMsgQueue, out sbMessage);
                    string returnedString = Encoding.UTF8.GetString(sbMessage.Body.ToArray());
                    bool passed = returnedString.Equals(message, StringComparison.CurrentCulture);
                    Assert.IsTrue(passed);
                }
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void SendStringToServiceBusTopicViaOperator()
        {
            string TopicName = "test-" + "SendActorStringToServiceBusTopicViaOperator".ToLower();
            // we cannot send messages to topics that have no subscriptions in them
            var proc = this.StartProcessing(TopicName, "test");
            try
            {
                string message = "HelloWorld";

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicPublishOperator<string>(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(message).Wait();

                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                var retMsgs = Encoding.UTF8.GetString(sbMessage.Body.ToArray());
                Assert.IsTrue((retMsgs).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void RecieveStringFromServiceBusTopic()
        {
            string subscription = "firstsubsccription";
            string TopicName = "test-" + "RecieveStringServiceBusTopic".ToLower();
            var Topic = GetServiceBusTopicSender(TopicName);
            try
            {
                CreateTopicAndSubscriptions(TopicName, new string[] { subscription });

                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                Topic.SendMessageAsync(new ServiceBusMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicSubscriptionOperator<string>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, TopicName, subscription, false),
                    (string foo) => { Assert.IsTrue(message.Equals(foo, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void SendBytesToServiceBusTopic()
        {
            string TopicName = "test-" + "SendBytesToServiceBusTopic".ToLower();
            // we cannot send messages to topics that have no subscriptions in them
            var proc = this.StartProcessing(TopicName, "test");
            try
            {
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToServiceBusTopic>(() => new SendBytesToServiceBusTopic(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                ServiceBusReceivedMessage sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body.ToArray()).Equals(message));
            }
            finally
            {
                proc.StopProcessingAsync().Wait();
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void RecieveBytesFromServiceBusTopic()
        {
            string subscription = "firstsubscription";
            string TopicName = "test-" + "RecieveBytesFromServiceBusTopic".ToLower();
            var Topic = GetServiceBusTopicSender(TopicName);
            try
            {
                CreateTopicAndSubscriptions(TopicName, new string[] { subscription });

                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                Topic.SendMessageAsync(new ServiceBusMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicSubscriptionOperator<byte[]>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, TopicName, subscription, false),
                    (ValueTypeMessage<byte> foo) => { Assert.IsTrue(message.Equals(Encoding.UTF8.GetString((byte[])foo.Message), StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }
        #endregion
    }
}
