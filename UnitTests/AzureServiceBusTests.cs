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
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.Azure.ServiceBus;
    using Mossharbor.AzureWorkArounds.ServiceBus;
    using Microsoft.Azure.EventHubs;
    using System.Threading;
    using System.Collections.Concurrent;
    using Microsoft.Azure.ServiceBus.Core;
    using System.IO;

    [TestClass]
    public class ServiceBusTests
    {

        #region ServiceBus Helpers
        ConcurrentQueue<Message> sbMsgQueue = new ConcurrentQueue<Message>();

        private QueueClient GetServiceBusQueue(string serviceBusQueueName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.ServiceBusConnectionString);
            QueueDescription qd;
            if (!ns.QueueExists(serviceBusQueueName, out qd))
                ns.CreateQueue(serviceBusQueueName);

            return new QueueClient(Connections.ServiceBusConnectionString, serviceBusQueueName);
        }

        private Microsoft.Azure.ServiceBus.Core.MessageSender GetServiceBusTopicSender(string topicName)
        {
            return new MessageSender(Connections.ServiceBusConnectionString, topicName, null);
        }

        private Microsoft.Azure.ServiceBus.Core.MessageReceiver GetServiceBusTopicReciever(string topicName, string subscription)
        {
            MessageReceiver reciever = new MessageReceiver(Connections.ServiceBusConnectionString, EntityNameHelper.FormatSubscriptionPath(topicName, subscription));
            MessageHandlerOptions options = new MessageHandlerOptions(HandleExceptions);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = 1;

            reciever.RegisterMessageHandler(RecieveMessages, options);
            return reciever;
        }

        private void CreateTopicAndSubscriptions(string topicName, string[] subscriptions)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.ServiceBusConnectionString);
            TopicDescription qd;
            if (!ns.TopicExists(topicName, out qd))
                ns.CreateTopic(topicName);

            CreateSubscriptions(topicName, subscriptions);

        }

        private void CreateSubscriptions(string topicName, string[] subscriptions)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.ServiceBusConnectionString);

            if (null != subscriptions && subscriptions.Any())
            {
                Parallel.ForEach(subscriptions, subscription =>
                {
                    SubscriptionDescription sd;
                    if (!ns.SubscriptionExists(topicName, subscription, out sd))
                        ns.CreateSubscription(topicName, subscription);
                });
            }

        }

        private void DeleteServiceBusQueue(string serviceBusQueueName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.ServiceBusConnectionString);
            ns.DeleteQueue(serviceBusQueueName);
        }

        private void DeleteServiceBusTopic(string topic)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.ServiceBusConnectionString);
            TopicDescription qd;
            if (!ns.TopicExists(topic, out qd))
                ns.DeleteTopic(topic);
        }

        private Task RecieveMessages(Message sbMessage, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                sbMsgQueue.Enqueue(sbMessage);
            });
        }

        private Task HandleExceptions(Microsoft.Azure.ServiceBus.ExceptionReceivedEventArgs eargs)
        {
            return Task.Factory.StartNew(() =>
            {
                Assert.Fail(eargs.Exception.Message);
            });
        }
        
        private void WaitForQueue<T>(ConcurrentQueue<T> queue, out T data) where T : class
        {
            int attempts = 0;
            data = default(T);
            while (!queue.TryDequeue(out data) && attempts < 10)
            {
                ++attempts;
                System.Threading.Thread.Sleep(1000);
            }
            if (attempts >= 10)
                Assert.Fail("We took way too long to get data back from the queue");
        }

        #endregion

        #region Service Bus Queue
        [TestMethod]
        public void SendActorMessageToServiceBusQueue()
        {
            string queueName = "test-" + "SendActorMessageToServiceBusQueue".ToLower();
            var queue = GetServiceBusQueue(queueName);
            try
            {
                queue.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToServiceBusQueue>(() => new SendMessageToServiceBusQueue(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void SendActorMessageToServiceBusQueueViaOperator()
        {
            string queueName = "test-" + "SendActorMessageToServiceBusQueueViaOperator".ToLower();
            var queue = GetServiceBusQueue(queueName);
            try
            {
                queue.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });
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
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromServiceBusQueue()
        {
            string queueName = "test-" + "RecieveActorMessageFromServiceBusQueue".ToLower();
            var queue = GetServiceBusQueue(queueName);
            try
            {
                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                queue.SendAsync(new Message(msgBytes));

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
            var queue = GetServiceBusQueue(queueName);
            try
            {
                queue.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });

                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToServiceBusQueue>(() => new SendStringToServiceBusQueue(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Ask(message).Wait();
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body).Equals(message, StringComparison.CurrentCulture));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void SendActorStringToServiceBusQueueViaOperator()
        {
            string queueName = "test-" + "SendActorStringToServiceBusQueueViaOperator".ToLower();
            var queue = GetServiceBusQueue(queueName);
            try
            {
                queue.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });
                string message = "HelloWorld";

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusQueuePublishOperator<string>(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(message).Wait();

                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                var retMsgs = Encoding.UTF8.GetString(sbMessage.Body);
                Assert.IsTrue((retMsgs).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void RecieveStringFromServiceBusQueue()
        {
            string queueName = "test-" + "RecieveStringServiceBusQueue".ToLower();
            var queue = GetServiceBusQueue(queueName);
            try
            {
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                queue.SendAsync(new Message(msgBytes));

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
            var queue = GetServiceBusQueue(queueName);
            try
            {
                queue.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });

                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToServiceBusQueue>(() => new SendBytesToServiceBusQueue(Connections.ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusQueue(queueName);
            }
        }

        [TestMethod]
        public void RecieveBytesFromServiceBusQueue()
        {
            string queueName = "test-" + "RecieveBytesFromServiceBusQueue".ToLower();
            var queue = GetServiceBusQueue(queueName);
            try
            {
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                queue.SendAsync(new Message(msgBytes));

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
            CreateTopicAndSubscriptions(TopicName, new string[] { "test" });
            var Topic = GetServiceBusTopicReciever(TopicName, "test");
            try
            {

                //Topic.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToServiceBusTopic>(() => new SendMessageToServiceBusTopic(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);
                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void SendActorMessageToServiceBusTopicViaOperator()
        {
            string TopicName = "test-" + "SendActorMessageToServiceBusTopicViaOperator".ToLower();
            // we cannot send messages to topics that have no subscriptions in them
            CreateTopicAndSubscriptions(TopicName, new string[] { "test" });
            var Topic = GetServiceBusTopicReciever(TopicName, "test");
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
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(sbMessage.Body);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
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
                Topic.SendAsync(new Message(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, TopicName, subscription, false, 2),
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
            CreateTopicAndSubscriptions(TopicName, new string[] { "test" });
            var Topic = GetServiceBusTopicReciever(TopicName, "test");
            try
            {
                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToServiceBusTopic>(() => new SendStringToServiceBusTopic(Connections.ServiceBusConnectionString, TopicName, true));

                for (int i = 0; i < 100; ++i)
                {
                    Telegraph.Instance.Ask(message).Wait();
                    Message sbMessage = null;
                    WaitForQueue(sbMsgQueue, out sbMessage);
                    string returnedString = Encoding.UTF8.GetString(sbMessage.Body);
                    bool passed = returnedString.Equals(message, StringComparison.CurrentCulture);
                    Assert.IsTrue(passed);
                }
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteServiceBusTopic(TopicName);
            }
        }

        [TestMethod]
        public void SendStringToServiceBusTopicViaOperator()
        {
            string TopicName = "test-" + "SendActorStringToServiceBusTopicViaOperator".ToLower();
            // we cannot send messages to topics that have no subscriptions in them
            CreateTopicAndSubscriptions(TopicName, new string[] { "test" });
            var Topic = GetServiceBusTopicReciever(TopicName, "test");
            try
            {
                string message = "HelloWorld";

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicPublishOperator<string>(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(message).Wait();

                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                var retMsgs = Encoding.UTF8.GetString(sbMessage.Body);
                Assert.IsTrue((retMsgs).Equals(message));
            }
            finally
            {
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
                Topic.SendAsync(new Message(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicSubscriptionOperator<string>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, TopicName, subscription, false, 2),
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
            CreateTopicAndSubscriptions(TopicName, new string[] { "test" });
            var Topic = GetServiceBusTopicReciever(TopicName, "test");
            try
            {
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToServiceBusTopic>(() => new SendBytesToServiceBusTopic(Connections.ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body).Equals(message));
            }
            finally
            {
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
                Topic.SendAsync(new Message(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicSubscriptionOperator<byte[]>(LocalConcurrencyType.DedicatedThreadCount, Connections.ServiceBusConnectionString, TopicName, subscription, false, 2),
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
