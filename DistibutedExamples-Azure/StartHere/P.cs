﻿using System;

namespace StartHere
{
    using Microsoft.ServiceBus;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System.Text;
    using Telegraphy.Azure;
    using Telegraphy.Net;

    class P
    {
        static string queueAccountKey = @"E8vxv+2T+TKMfGBDYoWT8rSt0NINfoUOU8KP8AHmdTi8+dBdjIweeH3UvYfq6dA1PDtB3ky52hl0ZlAx3g1R6A==";
        static string queueAccountConnectionString = @"DefaultEndpointsProtocol=https;AccountName=telegraphytest;AccountKey=" + queueAccountKey + ";EndpointSuffix=core.windows.net";

        static string serviceBusKey = "kCxvZWTdqMCSVjsur+MTiB1J3MwV0p8Cq3eRlZm9HUk=";
        static string serviceBusConnectionString = @"Endpoint=sb://telagraphytest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey="+ serviceBusKey+"";

        static string eventHubConnectionString = @"Endpoint=sb://telagraphyeventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xB1cB7sxM0KlaE1XZbGq/JXZAESC+Pk504RJndkV8R4=";

        static void Main(string[] args)
        {
            #region StorageQueues
            SimpleSendStringDataToAzureStorageQueue(queueAccountConnectionString, "simpleazureexample");
            //SimpleSendRawDataToAzureStorageQueue(queueAccountConnectionString, "simpleazureexample");

            //SimpleSendToAzureStorageQueue(queueAccountConnectionString", "simpleazureexample");

            //SendToAzureQueue(queueAccountConnectionString, "useazurequeueexample");
            //RecieveFromAzureQueue(queueAccountConnectionString, "useazurequeueexample");

            //SendToSpecificQueues(queueAccountConnectionString, "Ping", "Pong");
            //RecieveFromSpecificQueues(queueAccountConnectionString, "Ping", "Pong");
            #endregion

            #region Service Bus
            //SendToAzureServiceBusQueue(serviceBusConnectionString, "useazurequeueexample");
            //RecieveFromAzureServiceBusQueue(serviceBusConnectionString, "useazurequeueexample");

            //NamespaceManager ns = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
            //if (!ns.SubscriptionExists("useazurequeuetopicexample", "Subscription1"))
            //    ns.CreateSubscription("useazurequeuetopicexample", "Subscription1");

            //SendToAzureServiceBusTopic(serviceBusConnectionString, "useazurequeuetopicexample");
            //RecieveFromAzureServiceBusTopic(serviceBusConnectionString, "useazurequeuetopicexample", "Subscription1");

            //SendToAzureServiceBusSubscription(serviceBusConnectionString, "useazurequeuetopic2example", new string[] { "Subscription1", "Subscription2", "Subscription3" });
            //RecieveFromAzureServiceBusSubscription(serviceBusConnectionString, "useazurequeuetopic2example", "Subscription1");
            //RecieveFromAzureServiceBusSubscription(serviceBusConnectionString, "useazurequeuetopic2example", "Subscription2");
            //RecieveFromAzureServiceBusSubscription(serviceBusConnectionString, "useazurequeuetopic2example", "Subscription3");

            //SendMessagesWithServiceBusProperties(serviceBusConnectionString, "topicwithpropertiesexample", "delayedmessageSubscription", TimeSpan.FromMinutes(1));
            //RecieveDelayedMessage(serviceBusConnectionString, "topicwithpropertiesexample", "delayedmessageSubscription",TimeSpan.FromSeconds(70));
            #endregion

            #region Event Bus

            #endregion
        }

        static void SimpleSendStringDataToAzureStorageQueue(string connectionString, string queueName)
        {
            // create message
            string message = @"Hello World";

            // Setup send to queue
            Telegraph.Instance.Register<string, SendStringToStorageQueue>(() => new SendStringToStorageQueue(connectionString, queueName, true));

            // Send message to queue
            Telegraph.Instance.Ask(message).Wait();
        }

        static void SimpleSendRawDataToAzureStorageQueue(string connectionString, string queueName)
        {
            // create message
            byte[] message = Encoding.UTF8.GetBytes(@"Hello World");

            // Setup send to queue
            //Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToStorageQueue>(() => new SendBytesToStorageQueue(connectionString, queueName, true));

            Telegraph.Instance.Register<byte[], SendBytesToStorageQueue>(() => new SendBytesToStorageQueue(connectionString, queueName, true));

            // Send message to queue
            Telegraph.Instance.Ask(message).Wait();
        }

        static void SimpleSendMessageToAzureStorageQueue(string connectionString, string queueName)
        {
            // Setup send to queue
            Telegraph.Instance.Register<PingPong.Ping, SendMessageToStorageQueue>(() => new SendMessageToStorageQueue(connectionString, queueName, true));

            // Set up serialization
            MessageSerializationActor serializer = new MessageSerializationActor();
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => serializer);

            // Send message to queue
            Telegraph.Instance.Ask(new PingPong.Ping()).Wait();

            // If you look on the queue the binary version of Ping will have been put you can use the Deserialization Actor to pull this message  down later
        }

        static void SendToAzureQueue(string connectionstring, string queueName)
        {
            long azureOperatorId = Telegraph.Instance.Register(new StorageQueueDeliveryOperator(connectionstring, queueName, true));
            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);

            MessageSerializationActor serializer = new MessageSerializationActor();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount,2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

            //NOTE: we route all non registered messages to the main operator
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Ask(new PingPong.Ping()).Wait();

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveFromAzureQueue(string connectionstring, string queueName)
        {
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator());
            long azureOperatorID = Telegraph.Instance.Register(new StorageQueueReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, connectionstring, queueName, false, 2));

            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(localOperatorID, () => deserializer);
            deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorID, ping =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(ping.Message);
            });

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 0, 30));
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void SendToSpecificQueues(string queueAccountConnectionString, string queue1Name, string queue2Name)
        {
            Telegraph.Instance.UnRegisterAll();

            long operator1ID = Telegraph.Instance.Register(new StorageQueueDeliveryOperator(queueAccountConnectionString, queue1Name, true));
            long operator2ID = Telegraph.Instance.Register(new StorageQueueDeliveryOperator(queueAccountConnectionString, queue2Name, true));
            Telegraph.Instance.Register<PingPong.Ping>(operator1ID);
            Telegraph.Instance.Register<PingPong.Pong>(operator2ID);

            // Indicate how we want to serialize the messages before putting them on the azure queue
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => new MessageSerializationActor());
            
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell(new PingPong.Pong());

            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell(new PingPong.Ping());
            
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveFromSpecificQueues(string queueAccountConnectionString, string queue1Name, string queue2Name)
        {
            long serializationOperator = Telegraph.Instance.Register(new LocalOperator());
            long operator1ID = Telegraph.Instance.Register(new StorageQueueReceptionOperator(LocalConcurrencyType.ActorsOnThreadPool, queueAccountConnectionString, queue1Name));
            long operator2ID = Telegraph.Instance.Register(new StorageQueueReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, queueAccountConnectionString, queue2Name, false, 3));

            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(serializationOperator, () => deserializer);
            deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
            deserializer.Register<PingPong.Pong>((object msg) => (PingPong.Pong)msg);

            Telegraph.Instance.Register<PingPong.Pong>(operator2ID, pong =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(pong.Message);
            });

            Telegraph.Instance.Register<PingPong.Ping>(operator1ID, ping =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(ping.Message);
            });


            Telegraph.Instance.WaitTillEmpty(new TimeSpan(1, 0, 30));
        }
        
        static void SendToAzureServiceBusQueue(string connectionString, string queueName)
        {
            long azureOperatorId = Telegraph.Instance.Register(new ServiceBusQueueDeliveryOperator(serviceBusConnectionString, queueName, true));
            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);

            MessageSerializationActor serializer = new MessageSerializationActor();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

            // NOTE: we route all non registered messages to the main operator
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Ask(new PingPong.Ping()).Wait();

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveFromAzureServiceBusQueue(string connectionString, string queueName)
        {
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator());
            long azureOperatorID = Telegraph.Instance.Register(new ServiceBusQueueReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, connectionString, queueName, true, 2));

            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(localOperatorID, () => deserializer);
            deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorID, ping =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(ping.Message);
            });
            
            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 0, 30));
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void SendToAzureServiceBusTopic(string connectionString, string topicName)
        {
            long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicDeliveryOperator(serviceBusConnectionString, topicName, true));
            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);

            MessageSerializationActor serializer = new MessageSerializationActor();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

            // NOTE: we route all non registered messages to the main operator
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Ask(new PingPong.Ping()).Wait();

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveFromAzureServiceBusTopic(string connectionString, string topicName, string subscription)
        {
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator());
            long azureOperatorID = Telegraph.Instance.Register(new ServiceBusTopicReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, connectionString, topicName, subscription, true, 2));

            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(localOperatorID, () => deserializer);
            deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorID, ping =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(subscription + ":"+ping.Message);
            });

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 0, 30));
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void SendToAzureServiceBusSubscription(string connectionString, string topicName, string[] subscriptionsToCreate)
        {
            long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicDeliveryOperator(serviceBusConnectionString, topicName, subscriptionsToCreate, true));
            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);

            MessageSerializationActor serializer = new MessageSerializationActor();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

            // NOTE: we route all non registered messages to the main operator
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Ask(new PingPong.Ping()).Wait();

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveFromAzureServiceBusSubscription(string connectionString, string topicName, string subscription)
        {
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator());
            long azureOperatorID = Telegraph.Instance.Register(new ServiceBusTopicReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, connectionString, topicName, subscription, true, 2));

            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(localOperatorID, () => deserializer);
            deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);

            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorID, pung =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(subscription + ":" + pung.Message);
            });

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 0, 30));
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void SendMessagesWithServiceBusProperties(string connectionString, string topicName, string subscriptionsToCreate, TimeSpan delay)
        {
            long azureOperatorId = Telegraph.Instance.Register(new ServiceBusTopicDeliveryOperator(serviceBusConnectionString, topicName, subscriptionsToCreate, true));
            Telegraph.Instance.Register<PingPong.Pung>(azureOperatorId);

            MessageSerializationActor serializer = new MessageSerializationActor();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.DedicatedThreadCount, 2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

            // enqueu in the topic in 1 hour the class Pung implments IServiceBusPropertiesProvider and sets EnequeueTime
            // this will tell azure to accept the message but only enqueue it after 1 minute elapses.
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Ask(new PingPong.Pung("Pung", DateTime.UtcNow.Add(delay))).Wait();

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveDelayedMessage(string connectionString, string topicName, string subscription, TimeSpan delay)
        {
            System.Threading.Thread.Sleep(delay);
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator());
            long azureOperatorID = Telegraph.Instance.Register(new ServiceBusTopicReceptionOperator(LocalConcurrencyType.DedicatedThreadCount, connectionString, topicName, subscription, true, 2));

            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(localOperatorID, () => deserializer);
            deserializer.Register<PingPong.Pung>((object msg) => (PingPong.Pung)msg);

            Telegraph.Instance.Register<PingPong.Pung>(azureOperatorID, pung =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(subscription + ":" + pung.Message);
            });

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 0, 30));
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }
        
    }
}
