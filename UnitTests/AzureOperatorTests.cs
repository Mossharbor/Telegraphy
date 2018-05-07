using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telegraphy.Net;
    using Telegraphy.Azure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.Azure.ServiceBus;
    using Mossharbor.AzureWorkArounds.ServiceBus;
    using System.Threading;
    using System.Collections.Concurrent;
    using Microsoft.Azure.ServiceBus.Core;

    [TestClass]
    public class AzureOperatorTests
    {
        static string serviceBusKey = "kCxvZWTdqMCSVjsur+MTiB1J3MwV0p8Cq3eRlZm9HUk=";
        static string storageAccountKey = @"E8vxv+2T+TKMfGBDYoWT8rSt0NINfoUOU8KP8AHmdTi8+dBdjIweeH3UvYfq6dA1PDtB3ky52hl0ZlAx3g1R6A==";

        private string ServiceBusConnectionString { get { return @"Endpoint=sb://telagraphytest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=" + serviceBusKey + ""; } }
        private string StorageConnectionString { get { return @"DefaultEndpointsProtocol=https;AccountName=telegraphytest;AccountKey=" + storageAccountKey + ";EndpointSuffix=core.windows.net"; } }
        private string EventHubConnectionString { get { return ""; } }
        ConcurrentQueue<Message> sbMsgQueue = new ConcurrentQueue<Message>();

        private CloudQueue GetStorageQueue(string queueName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName.ToLower());
            queue.CreateIfNotExists();
            return queue;
        }

        private QueueClient GetServiceBusQueue(string serviceBusQueueName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);
            QueueDescription qd;
            if (!ns.QueueExists(serviceBusQueueName, out qd))
                ns.CreateQueue(serviceBusQueueName);

            return new QueueClient(ServiceBusConnectionString, serviceBusQueueName);
        }

        private Microsoft.Azure.ServiceBus.Core.MessageSender GetServiceBusTopicSender(string topicName)
        {
            return new MessageSender(ServiceBusConnectionString, topicName, null);
        }

        private Microsoft.Azure.ServiceBus.Core.MessageReceiver GetServiceBusTopicReciever(string topicName,string subscription)
        {
            MessageReceiver reciever = new MessageReceiver(ServiceBusConnectionString, EntityNameHelper.FormatSubscriptionPath(topicName, subscription));
            MessageHandlerOptions options = new MessageHandlerOptions(HandleExceptions);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = 1;

            reciever.RegisterMessageHandler(RecieveMessages, options);
            return reciever;
        }

        private void CreateTopicAndSubscriptions(string topicName, string[] subscriptions)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);
            TopicDescription qd;
            if (!ns.TopicExists(topicName, out qd))
                ns.CreateTopic(topicName);

            CreateSubscriptions(topicName, subscriptions);

        }

        private void CreateSubscriptions(string topicName, string[] subscriptions)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);

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
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);
            ns.DeleteQueue(serviceBusQueueName);
        }

        private void DeleteServiceBusTopic(string topic)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(ServiceBusConnectionString);
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
        
        private Task HandleExceptions(ExceptionReceivedEventArgs eargs)
        {
            return Task.Factory.StartNew(() =>
            {
                Assert.Fail(eargs.Exception.Message);
            });
        }

        #region Storage Queue
        [TestMethod]
        public void SendActorMessageToStorageQueue()
        {
            string queueName = "test-" + "SendActorMessageToStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            try
            {
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToStorageQueue>(() => new SendMessageToStorageQueue(StorageConnectionString, queueName, true));
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => serializer);

                Telegraph.Instance.Ask(aMsg).Wait();
                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                DeSerializeMessage dMsg = new DeSerializeMessage(queue.GetMessage().AsBytes);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromStorageQueue()
        {
            string queueName = "test-" + "RecieveActorMessageFromStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage sMsg = new SerializeMessage(actorMessage);
                MessageSerializationActor serializer = new MessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                queue.AddMessage(new CloudQueueMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueActorMessageReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, StorageConnectionString, queueName, false, 2),
                    (PingPong.Ping foo) => { Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
            }
        }

        [TestMethod]
        public void SendStringToStorageQueue()
        {
            string queueName = "test-" + "SendStringStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            try
            {
                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToStorageQueue>(() => new SendStringToStorageQueue(StorageConnectionString, queueName, true));
                Telegraph.Instance.Ask(message).Wait();
                Assert.IsTrue(queue.GetMessage().AsString.Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
            }
        }

        [TestMethod]
        public void RecieveStringFromStorageQueue()
        {
            string queueName = "test-"+"RecieveStringStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                queue.AddMessage(new CloudQueueMessage(message));

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueStringReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, StorageConnectionString, queueName, false, 2),
                    (string foo)=> { Assert.IsTrue(message.Equals(foo, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName+"-deadletter").DeleteAsync();
            }
        }

        [TestMethod]
        public void SendBytesToStorageQueue()
        {
            string queueName = "test-" + "SendBytesToStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            try
            {
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToStorageQueue>(() => new SendBytesToStorageQueue(StorageConnectionString, queueName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                Assert.IsTrue(Encoding.UTF8.GetString(queue.GetMessage().AsBytes).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
            }
        }

        [TestMethod]
        public void RecieveBytesFromStorageQueue()
        {
            string queueName = "test-" + "RecieveBytesFromStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                queue.AddMessage(new CloudQueueMessage(message));

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueByteArrayReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, StorageConnectionString, queueName, false, 2),
                    (ValueTypeMessage<byte> foo) => { Assert.IsTrue(message.Equals(Encoding.UTF8.GetString((byte[])foo.Message), StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
            }
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
                queue.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) {AutoComplete = false });
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToServiceBusQueue>(() => new SendMessageToServiceBusQueue(ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => serializer);

                Telegraph.Instance.Ask(aMsg).Wait();
                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
                DeSerializeMessage dMsg = new DeSerializeMessage(sbMessage.Body);
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

                long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusQueueActorMessageDeliveryOperator(ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(aMsg).Wait();

                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
                DeSerializeMessage dMsg = new DeSerializeMessage(sbMessage.Body);
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
                SerializeMessage sMsg = new SerializeMessage(actorMessage);
                MessageSerializationActor serializer = new MessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                queue.SendAsync(new Message(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusQueueActorMessageReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, ServiceBusConnectionString, queueName, false, 2),
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
                Telegraph.Instance.Register<string, SendStringToServiceBusQueue>(() => new SendStringToServiceBusQueue(ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Ask(message).Wait();
                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body).Equals(message,StringComparison.CurrentCulture));
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

                long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusQueueStringDeliveryOperator(ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(message).Wait();
                
                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
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
                    new ServiceBusQueueStringReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, ServiceBusConnectionString, queueName, false, 2),
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

                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToServiceBusQueue>(() => new SendBytesToServiceBusQueue(ServiceBusConnectionString, queueName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
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
                    new ServiceBusQueueByteArrayReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, ServiceBusConnectionString, queueName, false, 2),
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
            CreateSubscriptions(TopicName, new string[] { "test" });
            var Topic = GetServiceBusTopicReciever(TopicName, "test");
            try
            {

                //Topic.RegisterMessageHandler(RecieveMessages, new MessageHandlerOptions(HandleExceptions) { AutoComplete = false });
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToServiceBusTopic>(() => new SendMessageToServiceBusTopic(ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => serializer);
                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

                Telegraph.Instance.Ask(aMsg).Wait();
                Message sbMessage = null;
                while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); }
                DeSerializeMessage dMsg = new DeSerializeMessage(sbMessage.Body);
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

                long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicActorMessageDeliveryOperator(ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(aMsg).Wait();

                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
                DeSerializeMessage dMsg = new DeSerializeMessage(sbMessage.Body);
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
                SerializeMessage sMsg = new SerializeMessage(actorMessage);
                MessageSerializationActor serializer = new MessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                Topic.SendAsync(new Message(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new ServiceBusTopicActorMessageReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, ServiceBusConnectionString, TopicName, subscription, false, 2),
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
                Telegraph.Instance.Register<string, SendStringToServiceBusTopic>(() => new SendStringToServiceBusTopic(ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Ask(message).Wait();
                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
                Assert.IsTrue(Encoding.UTF8.GetString(sbMessage.Body).Equals(message, StringComparison.CurrentCulture));
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

                long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicStringDeliveryOperator(ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

                Telegraph.Instance.Ask(message).Wait();

                Message sbMessage = null;
                 while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
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
                    new ServiceBusTopicStringReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, ServiceBusConnectionString, TopicName, subscription, false,  2),
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

                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToServiceBusTopic>(() => new SendBytesToServiceBusTopic(ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                Message sbMessage = null;
                while (!sbMsgQueue.TryDequeue(out sbMessage)) { System.Threading.Thread.Sleep(100); } { }
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
                    new ServiceBusTopicByteArrayReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, ServiceBusConnectionString, TopicName, subscription, false, 2),
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
