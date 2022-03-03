using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Azure.Storage
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telegraphy.Net;
    using Telegraphy.Azure;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Queues;
    using global::Azure.Storage.Blobs.Specialized;
    using global::Azure.Messaging.ServiceBus;
    
    using global::Azure.Messaging.EventHubs.Processor;
    using global::Azure.Messaging.EventHubs;
    using System.Threading;
    using System.Collections.Concurrent;
    
    using System.IO;

    [TestClass]
    public class AzureStorageTests
    {
        private string StorageContainerName = "telegraphytest";
        private string TableStorageName = "telegraphytesttable";

        private global::Azure.Storage.Queues.QueueClient GetStorageQueue(string queueName)
        {
            var queue = new global::Azure.Storage.Queues.QueueClient(Connections.StorageConnectionString, queueName.ToLower());

            queue.CreateIfNotExists();
            return queue;
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
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToStorageQueue<PingPong.Ping>>(() => new SendMessageToStorageQueue<PingPong.Ping>(Connections.StorageConnectionString, queueName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 10, 0)))
                    Assert.Fail("Waited too long to send a message");

                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                var base64String = queue.ReceiveMessage().Value.Body.ToString();
                var messageBytes = Convert.FromBase64String(base64String);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(messageBytes);
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
                SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                string base64MsgBytes = Convert.ToBase64String(msgBytes);
                queue.SendMessage(new BinaryData(base64MsgBytes));
                ManualResetEvent received = new ManualResetEvent(false);

                Telegraph.Instance.Register<DeserializeMessage<IActorMessage>, IActorMessageDeserializationActor>(() => new IActorMessageDeserializationActor());

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, Connections.StorageConnectionString, queueName, false, 2),
                    (PingPong.Ping foo) => 
                    {
                        Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture));
                        received.Set();
                    });

                Assert.IsTrue(received.WaitOne(TimeSpan.FromSeconds(13), true), "We did not receive the message");
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromStorageQueuePostRegister()
        {
            string queueName = "test-" + "RecieveActorMessageFromStorageQueuePostRegister".ToLower();
            var queue = GetStorageQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                var actorMessage = new PingPong.Ping(message);
                SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                serializer.OnMessageRecieved(sMsg);
                byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                string base64MsgBytes = Convert.ToBase64String(msgBytes);
                ManualResetEvent received = new ManualResetEvent(false);

                Telegraph.Instance.Register<DeserializeMessage<IActorMessage>, IActorMessageDeserializationActor>(() => new IActorMessageDeserializationActor());

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, Connections.StorageConnectionString, queueName, false, 2),
                    (PingPong.Ping foo) =>
                    {
                        Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture));
                        received.Set();
                    });

                queue.SendMessage(new BinaryData(base64MsgBytes));

                Assert.IsTrue(received.WaitOne(TimeSpan.FromSeconds(3), true), "We did not receive the message");
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
                Telegraph.Instance.Register<string, SendStringToStorageQueue>(() => new SendStringToStorageQueue(Connections.StorageConnectionString, queueName, true));
                Telegraph.Instance.Ask(message).Wait();
                string queuedMessage = queue.ReceiveMessage().Value.Body.ToString();
                Assert.IsTrue(queuedMessage.Equals(message));
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
            string queueName = "test-" + "RecieveStringStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                queue.SendMessage(message);

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueSubscriptionOperator<string>(LocalConcurrencyType.DedicatedThreadCount, Connections.StorageConnectionString, queueName, false, 2),
                    (string foo) => { Assert.IsTrue(message.Equals(foo, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetStorageQueue(queueName).DeleteAsync();
                GetStorageQueue(queueName + "-deadletter").DeleteAsync();
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

                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToStorageQueue>(() => new SendBytesToStorageQueue(Connections.StorageConnectionString, queueName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();

                var base64String = queue.ReceiveMessage().Value.Body.ToString();
                var recievedMessageBytes = Convert.FromBase64String(base64String);

                Assert.IsTrue(Encoding.UTF8.GetString(recievedMessageBytes).Equals(message));
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
                queue.SendMessage(message);

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueSubscriptionOperator<byte[]>(LocalConcurrencyType.DedicatedThreadCount, Connections.StorageConnectionString, queueName, false, 2),
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


        #region Blob Storage

        public void TestSendStreamToStorage(Action setupFunction, string firstFile)
        {
            try
            {
                setupFunction();

                Telegraph.Instance.Ask((System.IO.Stream)System.IO.File.OpenRead(firstFile)).Wait();

                var acct = new BlobServiceClient(Connections.StorageConnectionString);
                var container = acct.GetBlobContainerClient(StorageContainerName);
                var blob = container.GetBlobClient(System.IO.Path.GetFileName(firstFile));

                Assert.IsTrue(blob.Exists());
                blob.Delete();
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
            }

        }

        public void TestSendFileToStorage(Action setupFunction)
        {
            try
            {
                string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();

                setupFunction();

                Telegraph.Instance.Ask(firstFile).Wait();

                var acct = new BlobServiceClient(Connections.StorageConnectionString);
                var container = acct.GetBlobContainerClient(StorageContainerName);
                var blob = container.GetBlobClient(System.IO.Path.GetFileName(firstFile));

                Assert.IsTrue(blob.Exists());
                blob.Delete();
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
            }

        }

        public void TestSendBytesToStorage(Action setupFunction, byte[] data = null)
        {
            try
            {
                string stringToSend = "Foobar";
                byte[] msgBytes = (null == data) ? Encoding.UTF8.GetBytes(stringToSend) : data;

                setupFunction();

                Telegraph.Instance.Ask(msgBytes.ToActorMessage()).Wait();

                var acct = new BlobServiceClient(Connections.StorageConnectionString);
                var container = acct.GetBlobContainerClient(StorageContainerName);
                var blob = container.GetBlobClient("0.txt");

                Assert.IsTrue(blob.Exists());
                blob.Delete();
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
            }
        }

        public void TestSendStringToStorage(Action setupFunction, string stringToSend)
        {
            try
            {
                int index = 0;
                setupFunction();

                Telegraph.Instance.Ask(stringToSend).Wait();

                var acct = new BlobServiceClient(Connections.StorageConnectionString);
                var container = acct.GetBlobContainerClient(StorageContainerName);
                var blob = container.GetBlockBlobClient("0.txt");

                Assert.IsTrue(blob.Exists());
                blob.Delete();
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void SendStreamToBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();

            TestSendStreamToStorage(() =>
            {
                Telegraph.Instance.Register<System.IO.Stream, SendStreamToBlobStorage>(
                () => new Telegraphy.Azure.SendStreamToBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    () => System.IO.Path.GetFileName(firstFile)));
            }
            , firstFile);
        }

        [TestMethod]
        public void SendStreamToAppendBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();

            TestSendStreamToStorage(() =>
            {
                Telegraph.Instance.Register<System.IO.Stream, SendStreamToAppendBlobStorage>(
                () => new Telegraphy.Azure.SendStreamToAppendBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    true,
                    () => System.IO.Path.GetFileName(firstFile)));
            }
            , firstFile);
        }

        [TestMethod]
        public void SendStreamToPageBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();

            Telegraph.Instance.Register<System.IO.MemoryStream, SendStreamToPageBlobStorage>(
            () => new Telegraphy.Azure.SendStreamToPageBlobStorage(
                Connections.StorageConnectionString,
                StorageContainerName,
                true,
                512,
                () => System.IO.Path.GetFileName(firstFile)));


            string stringToSend = "";
            for (int i = 0; i < 512; ++i)
                stringToSend += 'a';

            byte[] buffer = Encoding.UTF8.GetBytes(stringToSend);
            Telegraph.Instance.Ask(new System.IO.MemoryStream(buffer)).Wait();

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetBlockBlobClient(System.IO.Path.GetFileName(firstFile));

            Assert.IsTrue(blob.Exists());
            blob.Delete();
        }

        [TestMethod]
        public void SendFileToBlobStorage()
        {
            TestSendFileToStorage(() =>
            {
                Telegraph.Instance.Register<string, SendFileToBlobStorage>(
                () => new Telegraphy.Azure.SendFileToBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    (string fileName) => System.IO.Path.GetFileName(fileName)));
            });
        }

        [TestMethod]
        public void SendFileToAppendBlobStorage()
        {
            TestSendFileToStorage(() =>
            {
                Telegraph.Instance.Register<string, SendFileToAppendBlobStorage>(
                () => new Telegraphy.Azure.SendFileToAppendBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    true,
                    (string fileName) => System.IO.Path.GetFileName(fileName)));
            });
        }

        [TestMethod]
        public void SendStringToBlobStorage()
        {
            string stringToSend = "Foobar";

            int index = 0;
            TestSendStringToStorage(() =>
            {
                Telegraph.Instance.Register<string, SendStringToBlobStorage>(
                    () => new Telegraphy.Azure.SendStringToBlobStorage(
                        Connections.StorageConnectionString,
                        StorageContainerName,
                        () => index.ToString() + ".txt"));
            },
             stringToSend);
        }

        [TestMethod]
        public void SendStringToPageBlobStorage()
        {
            string stringToSend = "";
            for (int i = 0; i < 512; ++i)
                stringToSend += 'a';

            int index = 0;
            TestSendStringToStorage(() =>
            {
                Telegraph.Instance.Register<string, SendStringToPageBlobStorage>(
                    () => new Telegraphy.Azure.SendStringToPageBlobStorage(
                        Connections.StorageConnectionString,
                        StorageContainerName,
                        true,
                        512,
                        () => index.ToString() + ".txt"));
            },
             stringToSend);
        }

        [TestMethod]
        public void SendStringToAppendBlobStorage()
        {
            string stringToSend = "Foobar";

            int index = 0;
            TestSendStringToStorage(() =>
            {
                Telegraph.Instance.Register<string, SendStringToAppendBlobStorage>(
                    () => new Telegraphy.Azure.SendStringToAppendBlobStorage(
                        Connections.StorageConnectionString,
                        StorageContainerName,
                        true,
                        () => index.ToString() + ".txt"));
            },
             stringToSend);
        }

        [TestMethod]
        public void SendBytesToBlobStorage()
        {
            int index = 0;
            TestSendBytesToStorage(() =>
            {
                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToBlobStorage>(
               () => new Telegraphy.Azure.SendBytesToBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName,
                   () => index.ToString() + ".txt"));
            });
        }

        [TestMethod]
        public void SendBytesToAppendBlobStorage()
        {
            int index = 0;
            TestSendBytesToStorage(() =>
            {
                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToAppendBlobStorage>(
               () => new Telegraphy.Azure.SendBytesToAppendBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName,
                    true,
                   () => index.ToString() + ".txt"));
            });
        }

        [TestMethod]
        public void SendBytesToPageBlobStorage()
        {
            int index = 0;
            TestSendBytesToStorage(() =>
            {
                Telegraph.Instance.Register<ValueArrayTypeMessage<byte>, SendBytesToPageBlobStorage>(
               () => new Telegraphy.Azure.SendBytesToPageBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName,
                   true,
                   512,
                   () => index.ToString() + ".txt"));
            },
            new byte[512]); // page blob bytes must be in multiples of 512
        }

        [TestMethod]
        public void RecieveBytesFromBlobStorage()
        {
            string stringToSend = "RecieveBytesFromBlobStorage";

            Telegraph.Instance.Register<string, SendStringToBlobStorage>(
                () => new Telegraphy.Azure.SendStringToBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    () => "RecieveBytesFromBlobStorage.txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetBlockBlobClient("RecieveBytesFromBlobStorage.txt");

            Assert.IsTrue(blob.Exists());

            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<string, RecieveBytesFromBlobStorage>(
               () => new Telegraphy.Azure.RecieveBytesFromBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName));

            byte[] sentBytes = (byte[])Telegraph.Instance.Ask("RecieveBytesFromBlobStorage.txt").Result.ProcessingResult;
            string sentString = Encoding.UTF8.GetString(sentBytes);

            if (null != blob && blob.Exists())
                blob.Delete();
            Assert.IsTrue(sentString.Equals(stringToSend));
            Telegraph.Instance.UnRegisterAll();
        }

        [TestMethod]
        public void RecieveStringFromBlobStorage()
        {
            string stringToSend = "RecieveStringFromBlobStorage";

            Telegraph.Instance.Register<string, SendStringToBlobStorage>(
                () => new Telegraphy.Azure.SendStringToBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    () => "RecieveStringFromBlobStorage.txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetBlockBlobClient("RecieveStringFromBlobStorage.txt");

            Assert.IsTrue(blob.Exists());

            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<string, RecieveStringFromBlobStorage>(
               () => new Telegraphy.Azure.RecieveStringFromBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName, Encoding.UTF8));

            string sentString = (string)Telegraph.Instance.Ask("RecieveStringFromBlobStorage.txt").Result.ProcessingResult;

            if (null != blob && blob.Exists())
                blob.Delete();
            Assert.IsTrue(sentString.Equals(stringToSend));
            Telegraph.Instance.UnRegisterAll();
        }

        [TestMethod]
        public void RecieveStreamFromBlobStorage()
        {
            string stringToSend = "RecieveStreamFromBlobStorage";

            Telegraph.Instance.Register<string, SendStringToBlobStorage>(
                () => new Telegraphy.Azure.SendStringToBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    () => "RecieveStreamFromBlobStorage.txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetBlockBlobClient("RecieveStreamFromBlobStorage.txt");

            Assert.IsTrue(blob.Exists());

            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<string, RecieveStreamFromBlobStorage>(
               () => new Telegraphy.Azure.RecieveStreamFromBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName));

            Stream sentStream = (Stream)Telegraph.Instance.Ask("RecieveStreamFromBlobStorage.txt").Result.ProcessingResult;

            StreamReader sr = new StreamReader(sentStream);
            string sentString = sr.ReadLine();

            if (null != blob && blob.Exists())
                blob.Delete();
            Assert.IsTrue(sentString.Equals(stringToSend));
            Telegraph.Instance.UnRegisterAll();
        }

        [TestMethod]
        public void RecieveFileFromBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Last();
            string dest = Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"));

            if (File.Exists(dest))
                File.Delete(dest);

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetBlockBlobClient(System.IO.Path.GetFileName(firstFile));

            try
            {
                using (StreamReader sr = new StreamReader(firstFile))
                {
                    blob.Upload(sr.BaseStream);
                }

                Telegraph.Instance.Register<string, RecieveFileFromBlobStorage>(
                    () => new Telegraphy.Azure.RecieveFileFromBlobStorage(
                        Connections.StorageConnectionString,
                        StorageContainerName,
                        true,
                        (string blobName) => Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"))));

                Telegraph.Instance.Ask(Path.GetFileName(firstFile)).Wait();
                Assert.IsTrue(File.Exists(dest));
            }
            finally
            {
                if (null != blob && blob.Exists())
                    blob.Delete();
                File.Delete(dest);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void RecieveFileFromAppendBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Last();
            string dest = Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"));

            if (File.Exists(dest))
                File.Delete(dest);

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetAppendBlobClient(System.IO.Path.GetFileName(firstFile));
            blob.CreateIfNotExists();

            try
            {
                using (StreamReader sr = new StreamReader(firstFile))
                {
                    blob.AppendBlock(sr.BaseStream);
                }

                Telegraph.Instance.Register<string, RecieveFileFromBlobStorage>(
                    () => new Telegraphy.Azure.RecieveFileFromBlobStorage(
                        Connections.StorageConnectionString,
                        StorageContainerName,
                        true,
                        (string blobName) => Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"))));

                Telegraph.Instance.Ask(Path.GetFileName(firstFile)).Wait();
                Assert.IsTrue(File.Exists(dest));
            }
            finally
            {
                if (null != blob && blob.Exists())
                    blob.Delete();
                File.Delete(dest);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void RecieveFileFromPageBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Last();
            string dest = Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"));

            if (File.Exists(dest))
                File.Delete(dest);

            var acct = new BlobServiceClient(Connections.StorageConnectionString);
            var container = acct.GetBlobContainerClient(StorageContainerName);
            var blob = container.GetPageBlobClient(System.IO.Path.GetFileName(firstFile));
            blob.Create(512);

            string stringToSend = "";
            for (int i = 0; i < 512; ++i)
                stringToSend += 'a';
            try
            {
                using (MemoryStream sr = new MemoryStream(Encoding.UTF8.GetBytes(stringToSend)))
                {
                    blob.UploadPages(sr, 0);
                }

                Telegraph.Instance.Register<string, RecieveFileFromBlobStorage>(
                    () => new Telegraphy.Azure.RecieveFileFromBlobStorage(
                        Connections.StorageConnectionString,
                        StorageContainerName,
                        true,
                        (string blobName) => Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"))));

                Telegraph.Instance.Ask(Path.GetFileName(firstFile)).Wait();
                Assert.IsTrue(File.Exists(dest));
            }
            finally
            {
                if (null != blob && blob.Exists())
                    blob.Delete();
                File.Delete(dest);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void TestBlobPipeline()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            IEnumerable<string> files = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Take(2);

            // register sending the file themselves to blob storage
            Telegraph.Instance.Register<Messages.UploadMessage, SendFileToBlobStorage>(
               () => new Telegraphy.Azure.SendFileToBlobStorage(
                   Connections.StorageConnectionString,
                   StorageContainerName,
                   (string fileName) => System.IO.Path.GetFileName(fileName)));

            // register sending the file names to blob storage to be appended as a list
            Telegraph.Instance.Register<Messages.AppendNameToFileMessage, SendStringToAppendBlobStorage>(
                () => new Telegraphy.Azure.SendStringToAppendBlobStorage(
                    Connections.StorageConnectionString,
                    StorageContainerName,
                    true,
                    () => "Mp3List.txt"));

            Telegraph.Instance.Register<Messages.DeleteAllBlobsMsg, DeleteAllBlobsInContainer>(
                () => new Telegraphy.Azure.DeleteAllBlobsInContainer(Connections.StorageConnectionString));

            var pipeline = Telegraphy.Net.Pipeline
                // We are creating a pipeline that takes in a string (filename) and returns an UploadMessage
                .Create<string, Messages.UploadMessage>((string fileName) =>
                {
                    return new Messages.UploadMessage(fileName);
                })
                // We are sending that upload message to be uploaded to blob storage
                .Next<string>((Messages.UploadMessage msg) =>
                {
                    // Telegraph.Instance.Ask(msg).Wait();
                    return (string)msg.Message;
                })
                // we are sending name of the file we uploaded returns an upload message
                .Next<Messages.AppendNameToFileMessage>((string fileName) =>
                {
                    return new Messages.AppendNameToFileMessage(fileName);
                })
                // We are creating a pipeline that takes in an UploadMessage (filename) appends it to the append blob storage file
                .Next<string>((Messages.AppendNameToFileMessage msg) =>
                {
                    Telegraph.Instance.Ask(msg).Wait();
                    return (string)msg.Message; // fileName
                });

            // Process all fo the files in the list
            var filesNames = pipeline.Process(files).ToArray();

            Telegraph.Instance.Ask(new Messages.DeleteAllBlobsMsg(StorageContainerName));
            Telegraph.Instance.UnRegisterAll();
        }

        public class Messages
        {
            public class UploadMessage : SimpleMessage<UploadMessage>
            {
                public UploadMessage(string fileName) { this.Message = fileName; }
            }

            public class AppendNameToFileMessage : SimpleMessage<AppendNameToFileMessage>
            {
                public AppendNameToFileMessage(string fileName) { this.Message = fileName + System.Environment.NewLine; }
            }

            public class DeleteAllBlobsMsg : SimpleMessage<DeleteAllBlobsMsg>
            {
                public DeleteAllBlobsMsg(string container) { this.Message = container; }
            }
        }
        #endregion

        #region Table storage
        [TestMethod]
        public void InsertStringIntoTableStorage()
        {
            Telegraph.Instance.Register<InsertStringIntoTableStorageMessage, SendITableEntityToTableStorage>(
                    () => new Telegraphy.Azure.SendITableEntityToTableStorage(
                        Connections.StorageConnectionString,
                        TableStorageName,
                        TableOperationType.InsertOrReplace));

            Telegraph.Instance.Register<RetrieveFromTableStorageMessage, RetrieveFromTableStorage<string>>(
                    () => new Telegraphy.Azure.RetrieveFromTableStorage<string>(
                        Connections.StorageConnectionString,
                        TableStorageName));

            Telegraph.Instance.Register<DeleteFromTableStorageMessage, DeleteFromTableStorage>(
                    () => new Telegraphy.Azure.DeleteFromTableStorage(
                        Connections.StorageConnectionString,
                        TableStorageName));

            Telegraph.Instance.Ask(new InsertStringIntoTableStorageMessage("foo", "bar", "hello")).Wait();
            Telegraph.Instance.Ask(new InsertStringIntoTableStorageMessage("foo", "bar2", "world")).Wait();
            string retString = (string)Telegraph.Instance.Ask(new RetrieveFromTableStorageMessage("foo", "bar")).Result.ProcessingResult;
            Telegraph.Instance.Ask(new DeleteFromTableStorageMessage("foo", "bar")).Wait();
            Telegraph.Instance.Ask(new DeleteFromTableStorageMessage("foo", "bar2")).Wait();
            Assert.IsTrue(retString.Equals("hello"));
        }
        #endregion
    }
}
