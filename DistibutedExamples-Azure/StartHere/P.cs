using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartHere
{
    using Telegraphy.Net;
    using Telegraphy.Azure;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage;

    class P
    {
        static string queueAccountKey = @"E8vxv+2T+TKMfGBDYoWT8rSt0NINfoUOU8KP8AHmdTi8+dBdjIweeH3UvYfq6dA1PDtB3ky52hl0ZlAx3g1R6A==";
        static string queueAccountConnectionString = @"DefaultEndpointsProtocol=https;AccountName=telegraphytest;AccountKey=" + queueAccountKey + ";EndpointSuffix=core.windows.net";

        static void Main(string[] args)
        {
            SendToAzureQueue(queueAccountConnectionString, "useazurequeueexample");
            RecieveFromAzureQueue(queueAccountConnectionString, "useazurequeueexample");

            //SendToSpecificQueues(queueAccountConnectionString, "Ping", "Pong");
            //RecieveFromSpecificQueues(queueAccountConnectionString, "Ping", "Pong");
        }

        static void SendToAzureQueue(string connectionstring, string queueName)
        {
            long azureOperatorId = Telegraph.Instance.Register(new StorageQueueMessageDeliveryOperator(connectionstring, queueName, true));
            Telegraph.Instance.Register<PingPong.Ping>(azureOperatorId);

            MessageSerializationActor serializer = new MessageSerializationActor();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.LimitedThreadCount,2));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(localOperatorID, () => serializer);

            //NOTE: we route all non registered messages to the main operator
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Ask(new PingPong.Ping()).Wait();

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RecieveFromAzureQueue(string connectionstring, string queueName)
        {
            // NOTE: run this after you run SendToAzureQueue();
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator());
            long azureOperatorID = Telegraph.Instance.Register(new StorageQueueMessageReceptionOperator(LocalConcurrencyType.LimitedThreadCount, connectionstring, queueName, false, 2));

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

            long operator1ID = Telegraph.Instance.Register(new StorageQueueMessageDeliveryOperator(queueAccountConnectionString, queue1Name, true));
            long operator2ID = Telegraph.Instance.Register(new StorageQueueMessageDeliveryOperator(queueAccountConnectionString, queue2Name, true));
            Telegraph.Instance.Register<PingPong.Ping>(operator1ID);
            Telegraph.Instance.Register<PingPong.Pong>(operator2ID);

            // Indicate how we want to serialize the messages before putting them on the azure queue
            long localOperatorID = Telegraph.Instance.Register(new LocalOperator(LocalConcurrencyType.LimitedThreadCount, 2));
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
            long operator1ID = Telegraph.Instance.Register(new StorageQueueMessageReceptionOperator(LocalConcurrencyType.ActorsOnThreadPool, queueAccountConnectionString, queue1Name));
            long operator2ID = Telegraph.Instance.Register(new StorageQueueMessageReceptionOperator(LocalConcurrencyType.LimitedThreadCount, queueAccountConnectionString, queue2Name, false, 3));

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

        static void CustomSerialization(CloudQueue queue)
        {
            Telegraph.Instance.MainOperator = new StorageQueueMessageDeliveryOperator(queue);
            // TODO: Indicate how we want to serialize the messages before putting them on the azure queue
            // Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => new MessageSerializationActor());

            //NOTE: we route all non registered messages to the main operator
            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell(new PingPong.Ping());

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 0, 30));
            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }
    }
}
