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
    using Microsoft.Azure.EventHubs.Processor;
    using Microsoft.Azure.EventHubs;
    using System.Threading;
    using System.Collections.Concurrent;
    using Microsoft.Azure.ServiceBus.Core;

    [TestClass]
    public class AzureTests
    {
        static string serviceBusKey = "kCxvZWTdqMCSVjsur+MTiB1J3MwV0p8Cq3eRlZm9HUk=";
        static string storageAccountKey = @"E8vxv+2T+TKMfGBDYoWT8rSt0NINfoUOU8KP8AHmdTi8+dBdjIweeH3UvYfq6dA1PDtB3ky52hl0ZlAx3g1R6A==";
        
        private string StorageContainerName = "telagraphytesteventhub";
        private string ServiceBusConnectionString { get { return @"Endpoint=sb://telagraphytest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=" + serviceBusKey + ""; } }
        private string StorageConnectionString { get { return @"DefaultEndpointsProtocol=https;AccountName=telegraphytest;AccountKey=" + storageAccountKey + ";EndpointSuffix=core.windows.net"; } }
        private string EventHubConnectionString { get { return "Endpoint=sb://telagraphyeventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xB1cB7sxM0KlaE1XZbGq/JXZAESC+Pk504RJndkV8R4="; } }

        #region Storage Queue ServiceBus and Event Hub Helpers
        ConcurrentQueue<Message> sbMsgQueue = new ConcurrentQueue<Message>();
        static ConcurrentQueue<EventData> ehMsgQueue = new ConcurrentQueue<EventData>();

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

        private void StartEventHubReciever(string eventHubName,string consumerGroup)
        {
            var eventProcessorHost = new EventProcessorHost(
                eventHubName,
                consumerGroup, //GroupPartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);


            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();
        }

        private void CreateEventHub(string eventHubName,string consumerGroup)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(EventHubConnectionString);
            EventHubDescription qd;
            if (!ns.EventHubExists(eventHubName, out qd))
                ns.CreateEventHub(eventHubName);

            ConsumerGroupDescription cgd;
            if (!ns.ConsumerGroupExists(eventHubName, consumerGroup, out cgd))
                ns.CreateConsumerGroup(eventHubName, consumerGroup);
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

        private void DeleteEventHub(string eventHubName)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(EventHubConnectionString);
            EventHubDescription qd;
            if (ns.EventHubExists(eventHubName, out qd))
                ns.DeleteEventHub(eventHubName);
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

        private void WaitForQueue<T>(ConcurrentQueue<T> queue, out T data) where T: class
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

        #region eventHub
        [TestMethod]
        public void SendActorMessageToEventHub()
        {
            string eventHubName = "test-" + "SendActorMessageToEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                StartEventHubReciever(eventHubName, consumerGroup);

                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToEventHub>(() => new SendMessageToEventHub(EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                
                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                EventData ehMessage = null;
                WaitForQueue(ehMsgQueue, out ehMessage);
                DeSerializeMessage dMsg = new DeSerializeMessage(ehMessage.Body.Array);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }
        
        [TestMethod]
        public void SendActorMessageToEventHubViaOperator()
        {
            string eventHubName = "test-" + "SendActorMessageToEventHubViaOperator".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                StartEventHubReciever(eventHubName, consumerGroup);
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);

                long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new EventHubActorMessageDeliveryOperator(EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                EventData ehMessage = null;
                WaitForQueue(ehMsgQueue, out ehMessage);

                DeSerializeMessage dMsg = new DeSerializeMessage(ehMessage.Body.Array);
                deserializer.Ask(dMsg);
                var retMsgs = (PingPong.Ping)dMsg.ProcessingResult;
                Assert.IsTrue(((string)retMsgs.Message).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromEventHub()
        { 
            string eventHubName = "test-" + "RecieveActorMessageFromEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString) { EntityPath = eventHubName };
            EventHubClient queue = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            try
            {
                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage sMsg = new SerializeMessage(actorMessage);
                MessageSerializationActor serializer = new MessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                queue.SendAsync(new EventData(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                     new EventHubActorMessageReceptionOperator(EventHubConnectionString, eventHubName, false),
                    (PingPong.Ping foo) => { Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }

        [TestMethod]
        public void SendStringToEventHub()
        {
            string eventHubName = "test-" + "SendStringToEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                StartEventHubReciever(eventHubName, consumerGroup);

                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToEventHub>(() => new SendStringToEventHub(EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Ask(message).Wait();
                EventData ehMessage = null;
                WaitForQueue(ehMsgQueue, out ehMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.Array).Equals(message, StringComparison.CurrentCulture));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }
        
        [TestMethod]
        public void SendStringToEventHubViaOperator()
        {
            string eventHubName = "test-" + "SendStringToEventHubViaOperator".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                StartEventHubReciever(eventHubName, consumerGroup);
                string message = "HelloWorld";

               Telegraph.Instance.Register(new EventHubStringDeliveryOperator(EventHubConnectionString, eventHubName, true));
               if (!Telegraph.Instance.Ask(message).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                EventData ehMessage = null;
                WaitForQueue(ehMsgQueue, out ehMessage);

                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.Array).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }
        
        [TestMethod]
        public void RecieveStringFromEventHub()
        {
            string eventHubName = "test-" + "RecieveStringFromEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString) { EntityPath = eventHubName };
            EventHubClient queue = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            try
            {
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                queue.SendAsync(new EventData(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                     new EventHubStringReceptionOperator(EventHubConnectionString, eventHubName, false),
                    (string foo) => { Assert.IsTrue(message.Equals(foo, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }


        [TestMethod]
        public void SendBytesToEventHub()
        {
            string eventHubName = "test-" + "SendBytesToEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                StartEventHubReciever(eventHubName, consumerGroup);

                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToEventHub>(() => new SendBytesToEventHub(EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                EventData ehMessage = null;
                WaitForQueue(ehMsgQueue, out ehMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.Array).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }
        
        [TestMethod]
        public void SendBytesToEventHubViaOperator()
        {
            string eventHubName = "test-" + "SendBytesToEventHubViaOperator".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                StartEventHubReciever(eventHubName, consumerGroup);
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register(new EventHubByteArrayDeliveryOperator(EventHubConnectionString, eventHubName, true));

                if (!Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                EventData ehMessage = null;
                WaitForQueue(ehMsgQueue, out ehMessage);

                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.Array).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }

        [TestMethod]
        public void RecieveBytesFromEventHub()
        {
            string eventHubName = "test-" + "RecieveBytesFromEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString) { EntityPath = eventHubName };
            EventHubClient queue = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            try
            {
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                queue.SendAsync(new EventData(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                     new EventHubStringReceptionOperator(EventHubConnectionString, eventHubName, false),
                    (ValueTypeMessage<byte> foo) => { Assert.IsTrue(message.Equals(Encoding.UTF8.GetString(foo.ToArray()), StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }

        public class SimpleEventProcessor: IEventProcessor
        {
            public Task CloseAsync(PartitionContext context, CloseReason reason)
            {
                return Task.CompletedTask;
            }

            public Task OpenAsync(PartitionContext context)
            {
                return Task.CompletedTask;
            }

            public Task ProcessErrorAsync(PartitionContext context, Exception error)
            {
                return Task.CompletedTask;
            }

            public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
            {
                foreach (var eventData in messages)
                {
                    ehMsgQueue.Enqueue(eventData);
                }

                return context.CheckpointAsync();
            }
        }

        #endregion

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

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

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

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
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

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
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
                WaitForQueue(sbMsgQueue, out sbMessage);
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

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
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

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                MessageDeserializationActor deserializer = new MessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                Message sbMessage = null;
                WaitForQueue(sbMsgQueue, out sbMessage);
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

                long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicStringDeliveryOperator(ServiceBusConnectionString, TopicName, true));
                Telegraph.Instance.Register<string>(azureOperatorId);
                MessageSerializationActor serializer = new MessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

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

        #region blob storage

        [TestMethod]
        public void SendFileToBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();

            Telegraph.Instance.Register<string, SendFileToBlobStorage>(
                () => new Telegraphy.Azure.SendFileToBlobStorage(
                    StorageConnectionString,
                    StorageContainerName,
                    (string fileName) => System.IO.Path.GetFileName(fileName)));

            Telegraph.Instance.Ask(firstFile).Wait();
            
            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlockBlobReference(System.IO.Path.GetFileName(firstFile));

            Assert.IsTrue(blob.Exists());
            blob.Delete();
        }

        [TestMethod]
        public void SendStringToBlobStorage()
        {
            string stringToSend = "Foobar";

            int index = 0;
            Telegraph.Instance.Register<string, SendStringToBlobStorage>(
                () => new Telegraphy.Azure.SendStringToBlobStorage(
                    StorageConnectionString,
                    StorageContainerName,
                    () => index.ToString()+".txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlockBlobReference("0.txt");

            Assert.IsTrue(blob.Exists());
            blob.Delete();
        }
        #endregion
    }
}
