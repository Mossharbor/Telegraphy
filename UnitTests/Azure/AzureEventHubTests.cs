﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Azure.EventHub
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telegraphy.Net;
    using Telegraphy.Azure;
    
    using global::Azure.Messaging.EventHubs.Processor;
    using global::Azure.Messaging.EventHubs;
    using System.Collections.Concurrent;
    using global::Azure.Messaging.EventHubs.Producer;
    using global::Azure.Messaging.EventHubs.Consumer;
    using global::Azure.Messaging.EventHubs.Primitives;
    using global::Azure.Storage.Blobs;
    using Mossharbor.AzureWorkArounds.ServiceBus;

    [TestClass]
    public class AzureEventHubTests
    {
        public static string StorageContainerName = "telegraphytesteventhub";

        static ConcurrentQueue<EventData> ehMsgQueue = new ConcurrentQueue<EventData>();
        static bool recieving = false;
        static System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        static System.Threading.Tasks.Task recievingTask = null;

        private void WaitForQueue<T>(ConcurrentQueue<T> queue, out T data) where T : class
        {
            System.Threading.Thread.Sleep(1000);

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

        private void StartEventHubReciever(string eventHubName, string consumerGroup)
        {
            cts = new System.Threading.CancellationTokenSource();
            var b = new BlobContainerClient(Connections.StorageConnectionString, StorageContainerName);
            var e = new EventProcessorClient(b, consumerGroup, Connections.EventHubConnectionString, eventHubName);

            e.ProcessEventAsync += (ProcessEventArgs eventArgs) =>
            {
                System.Console.WriteLine("Recieved data");
                ehMsgQueue.Enqueue(eventArgs.Data);
                return Task.CompletedTask;
            };
            e.ProcessErrorAsync += (ProcessErrorEventArgs eventArgs) =>
            {
                System.Console.WriteLine(eventArgs.Exception.Message);
                return Task.CompletedTask;
            };

            e.StartProcessing(cts.Token);
        }

        private void CreateEventHub(string eventHubName, string consumerGroup)
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.EventHubConnectionString);
            EventHubDescription qd;
            if (!ns.EventHubExists(eventHubName, out qd))
                ns.CreateEventHub(eventHubName);

            ConsumerGroupDescription cgd;
            if (!ns.ConsumerGroupExists(eventHubName, consumerGroup, out cgd))
                ns.CreateConsumerGroup(eventHubName, consumerGroup);
        }

        private void DeleteEventHub(string eventHubName)
        {
            recieving = false;
            cts.Cancel();
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(Connections.EventHubConnectionString);
            EventHubDescription qd;
            if (ns.EventHubExists(eventHubName, out qd))
            {
                Console.WriteLine("Deleting Event Hub" + eventHubName);
                ns.DeleteEventHub(eventHubName);
            }
        }

        #region eventHub
        [TestMethod]
        public void SendActorMessageToEventHub()
        {
            string eventHubName = "test-" + "SendActorMessageToEventHub".ToLower();
            string consumerGroup = eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToEventHub<PingPong.Ping>>(() => new SendMessageToEventHub<PingPong.Ping>(Connections.EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                EventData ehMessage = null;
                StartEventHubReciever(eventHubName, consumerGroup);
                WaitForQueue(ehMsgQueue, out ehMessage);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(ehMessage.Body.ToArray());
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
                string message = "HelloWorld";
                PingPong.Ping aMsg = new PingPong.Ping(message);

                long localOperatorID = Telegraph.Instance.Register(new LocalQueueOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
                long azureOperatorId = Telegraph.Instance.Register(new EventHubPublishOperator<IActorMessage>(Connections.EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(localOperatorID, () => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                EventData ehMessage = null;
                StartEventHubReciever(eventHubName, consumerGroup);
                WaitForQueue(ehMsgQueue, out ehMessage);

                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(ehMessage.Body.ToArray());
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
            var client = new EventHubProducerClient(Connections.EventHubConnectionString, eventHubName);
            try
            {
                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;

                using (var eventBatch = client.CreateBatchAsync().Result)
                {
                    eventBatch.TryAdd(new EventData(msgBytes));
                    client.SendAsync(eventBatch).Wait();
                }

                long azureOperatorID = Telegraph.Instance.Register(
                     new EventHubSubscriptionOperator<IActorMessage>(Connections.EventHubConnectionString, eventHubName, false),
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
            string consumerGroup = @"$Default"; // eventHubName + Guid.NewGuid().ToString().Substring(0, 6);
            CreateEventHub(eventHubName, consumerGroup);
            try
            {

                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToEventHub>(() => new SendStringToEventHub(Connections.EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Ask(message).Wait();
                EventData ehMessage = null;
                StartEventHubReciever(eventHubName, consumerGroup);
                WaitForQueue(ehMsgQueue, out ehMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.ToArray()).Equals(message, StringComparison.CurrentCulture));
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
                string message = "HelloWorld";

                Telegraph.Instance.Register(new EventHubPublishOperator<string>(Connections.EventHubConnectionString, eventHubName, true));
                if (!Telegraph.Instance.Ask(message).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                EventData ehMessage = null;
                StartEventHubReciever(eventHubName, consumerGroup);
                WaitForQueue(ehMsgQueue, out ehMessage);

                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.ToArray()).Equals(message));
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
            var client = new EventHubProducerClient(Connections.EventHubConnectionString, eventHubName);
            try
            {
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);

                client.SendAsync(new EventData[] { new EventData(msgBytes) });

                long azureOperatorID = Telegraph.Instance.Register(
                     new EventHubSubscriptionOperator<string>(Connections.EventHubConnectionString, eventHubName, false),
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
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToEventHub>(() => new SendBytesToEventHub(Connections.EventHubConnectionString, eventHubName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                EventData ehMessage = null;
                StartEventHubReciever(eventHubName, consumerGroup);
                WaitForQueue(ehMsgQueue, out ehMessage);
                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.ToArray()).Equals(message));
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
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register(new EventHubPublishOperator<byte[]>(Connections.EventHubConnectionString, eventHubName, true));

                if (!Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");
                EventData ehMessage = null;
                StartEventHubReciever(eventHubName, consumerGroup);
                WaitForQueue(ehMsgQueue, out ehMessage);

                Assert.IsTrue(Encoding.UTF8.GetString(ehMessage.Body.ToArray()).Equals(message));
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
            var client = new EventHubProducerClient(Connections.EventHubConnectionString, eventHubName);
            try
            {
                string message = "HelloWorld";
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                client.SendAsync(new EventData[] { new EventData(msgBytes) });

                long azureOperatorID = Telegraph.Instance.Register(
                     new EventHubSubscriptionOperator<string>(Connections.EventHubConnectionString, eventHubName, false),
                    (ValueTypeMessage<byte> foo) => { Assert.IsTrue(message.Equals(Encoding.UTF8.GetString(foo.ToArray()), StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                DeleteEventHub(eventHubName);
            }
        }

        #endregion
    }
}
