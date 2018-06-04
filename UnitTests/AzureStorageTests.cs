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
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.Azure.ServiceBus;
    using Mossharbor.AzureWorkArounds.ServiceBus;
    using Microsoft.Azure.EventHubs.Processor;
    using Microsoft.Azure.EventHubs;
    using System.Threading;
    using System.Collections.Concurrent;
    using Microsoft.Azure.ServiceBus.Core;
    using System.IO;

    [TestClass]
    public class AzureStorageTests
    {
        private string StorageContainerName = "telagraphytesteventhub";
        private string TableStorageName = "telegraphytesttable";
        private string StorageConnectionString { get { return @""; } }

        private CloudQueue GetStorageQueue(string queueName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName.ToLower());
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
                Telegraph.Instance.Register<PingPong.Ping, SendMessageToStorageQueue>(() => new SendMessageToStorageQueue(StorageConnectionString, queueName, true));
                Telegraph.Instance.Register<SerializeMessage<IActorMessage>, IActorMessageSerializationActor>(() => serializer);

                if (!Telegraph.Instance.Ask(aMsg).Wait(new TimeSpan(0, 0, 10)))
                    Assert.Fail("Waited too long to send a message");

                IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
                deserializer.Register<PingPong.Ping>((object msg) => (PingPong.Ping)msg);
                DeserializeMessage<IActorMessage> dMsg = new DeserializeMessage<IActorMessage>(queue.GetMessage().AsBytes);
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
                queue.AddMessage(new CloudQueueMessage(msgBytes));

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, StorageConnectionString, queueName, false, 2),
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
            string queueName = "test-" + "RecieveStringStorageQueue".ToLower();
            var queue = GetStorageQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                queue.AddMessage(new CloudQueueMessage(message));

                long azureOperatorID = Telegraph.Instance.Register(
                    new StorageQueueSubscriptionOperator<string>(LocalConcurrencyType.DedicatedThreadCount, StorageConnectionString, queueName, false, 2),
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
                    new StorageQueueSubscriptionOperator<byte[]>(LocalConcurrencyType.DedicatedThreadCount, StorageConnectionString, queueName, false, 2),
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

                var acct = CloudStorageAccount.Parse(StorageConnectionString);
                var client = acct.CreateCloudBlobClient();
                var container = client.GetContainerReference(StorageContainerName);
                var blob = container.GetBlobReference(System.IO.Path.GetFileName(firstFile));

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

                var acct = CloudStorageAccount.Parse(StorageConnectionString);
                var client = acct.CreateCloudBlobClient();
                var container = client.GetContainerReference(StorageContainerName);
                var blob = container.GetBlobReference(System.IO.Path.GetFileName(firstFile));

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

                var acct = CloudStorageAccount.Parse(StorageConnectionString);
                var client = acct.CreateCloudBlobClient();
                var container = client.GetContainerReference(StorageContainerName);
                var blob = container.GetBlobReference("0.txt");

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

                var acct = CloudStorageAccount.Parse(StorageConnectionString);
                var client = acct.CreateCloudBlobClient();
                var container = client.GetContainerReference(StorageContainerName);
                var blob = container.GetBlobReference("0.txt");

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
                    StorageConnectionString,
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
                    StorageConnectionString,
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
                StorageConnectionString,
                StorageContainerName,
                () => System.IO.Path.GetFileName(firstFile)));


            string stringToSend = "";
            for (int i = 0; i < 512; ++i)
                stringToSend += 'a';

            byte[] buffer = Encoding.UTF8.GetBytes(stringToSend);
            Telegraph.Instance.Ask(new System.IO.MemoryStream(buffer)).Wait();

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlobReference(System.IO.Path.GetFileName(firstFile));

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
                    StorageConnectionString,
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
                    StorageConnectionString,
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
                        StorageConnectionString,
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
                        StorageConnectionString,
                        StorageContainerName,
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
                        StorageConnectionString,
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
                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToBlobStorage>(
               () => new Telegraphy.Azure.SendBytesToBlobStorage(
                   StorageConnectionString,
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
                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToAppendBlobStorage>(
               () => new Telegraphy.Azure.SendBytesToAppendBlobStorage(
                   StorageConnectionString,
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
                Telegraph.Instance.Register<ValueTypeMessage<byte>, SendBytesToPageBlobStorage>(
               () => new Telegraphy.Azure.SendBytesToPageBlobStorage(
                   StorageConnectionString,
                   StorageContainerName,
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
                    StorageConnectionString,
                    StorageContainerName,
                    () => "RecieveBytesFromBlobStorage.txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlobReference("RecieveBytesFromBlobStorage.txt");

            Assert.IsTrue(blob.Exists());

            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<string, RecieveBytesFromBlobStorage>(
               () => new Telegraphy.Azure.RecieveBytesFromBlobStorage(
                   StorageConnectionString,
                   StorageContainerName));

            byte[] sentBytes = (byte[])Telegraph.Instance.Ask("RecieveBytesFromBlobStorage.txt").Result.ProcessingResult;
            string sentString = Encoding.UTF8.GetString(sentBytes);

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
                    StorageConnectionString,
                    StorageContainerName,
                    () => "RecieveStringFromBlobStorage.txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlobReference("RecieveStringFromBlobStorage.txt");

            Assert.IsTrue(blob.Exists());

            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<string, RecieveStringFromBlobStorage>(
               () => new Telegraphy.Azure.RecieveStringFromBlobStorage(
                   StorageConnectionString,
                   StorageContainerName, Encoding.UTF8));

            string sentString = (string)Telegraph.Instance.Ask("RecieveStringFromBlobStorage.txt").Result.ProcessingResult;

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
                    StorageConnectionString,
                    StorageContainerName,
                    () => "RecieveStreamFromBlobStorage.txt"));

            Telegraph.Instance.Ask(stringToSend).Wait();

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlobReference("RecieveStreamFromBlobStorage.txt");

            Assert.IsTrue(blob.Exists());

            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<string, RecieveStreamFromBlobStorage>(
               () => new Telegraphy.Azure.RecieveStreamFromBlobStorage(
                   StorageConnectionString,
                   StorageContainerName));

            Stream sentStream = (Stream)Telegraph.Instance.Ask("RecieveStreamFromBlobStorage.txt").Result.ProcessingResult;

            StreamReader sr = new StreamReader(sentStream);
            string sentString = sr.ReadLine();

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

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetBlockBlobReference(System.IO.Path.GetFileName(firstFile));

            blob.UploadFromFile(firstFile);

            Telegraph.Instance.Register<string, RecieveFileFromBlobStorage>(
                () => new Telegraphy.Azure.RecieveFileFromBlobStorage(
                    StorageConnectionString,
                    StorageContainerName,
                    true,
                    (string blobName) => Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"))));

            Telegraph.Instance.Ask(Path.GetFileName(firstFile)).Wait();

            blob.Delete();
            Assert.IsTrue(File.Exists(dest));
            File.Delete(dest);
            Telegraph.Instance.UnRegisterAll();
        }

        [TestMethod]
        public void RecieveFileFromAppendBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Last();
            string dest = Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"));

            if (File.Exists(dest))
                File.Delete(dest);

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetAppendBlobReference(System.IO.Path.GetFileName(firstFile));

            blob.UploadFromFile(firstFile);

            Telegraph.Instance.Register<string, RecieveFileFromBlobStorage>(
                () => new Telegraphy.Azure.RecieveFileFromBlobStorage(
                    StorageConnectionString,
                    StorageContainerName,
                    true,
                    (string blobName) => Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"))));

            Telegraph.Instance.Ask(Path.GetFileName(firstFile)).Wait();

            blob.Delete();
            Assert.IsTrue(File.Exists(dest));
            File.Delete(dest);
            Telegraph.Instance.UnRegisterAll();
        }

        [TestMethod]
        public void RecieveFileFromPageBlobStorage()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string firstFile = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Last();
            string dest = Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"));

            if (File.Exists(dest))
                File.Delete(dest);

            var acct = CloudStorageAccount.Parse(StorageConnectionString);
            var client = acct.CreateCloudBlobClient();
            var container = client.GetContainerReference(StorageContainerName);
            var blob = container.GetPageBlobReference(System.IO.Path.GetFileName(firstFile));

            string stringToSend = "";
            for (int i = 0; i < 512; ++i)
                stringToSend += 'a';

            blob.UploadFromByteArray(Encoding.UTF8.GetBytes(stringToSend), 0, 512);

            Telegraph.Instance.Register<string, RecieveFileFromBlobStorage>(
                () => new Telegraphy.Azure.RecieveFileFromBlobStorage(
                    StorageConnectionString,
                    StorageContainerName,
                    true,
                    (string blobName) => Path.Combine(Directory.GetCurrentDirectory(), System.IO.Path.GetFileName(firstFile + ".new"))));

            Telegraph.Instance.Ask(Path.GetFileName(firstFile)).Wait();

            blob.Delete();
            Assert.IsTrue(File.Exists(dest));
            File.Delete(dest);
            Telegraph.Instance.UnRegisterAll();
        }

        [TestMethod]
        public void TestBlobPipeline()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            IEnumerable<string> files = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Take(2);

            // register sending the file themselves to blob storage
            Telegraph.Instance.Register<Messages.UploadMessage, SendFileToBlobStorage>(
               () => new Telegraphy.Azure.SendFileToBlobStorage(
                   StorageConnectionString,
                   StorageContainerName,
                   (string fileName) => System.IO.Path.GetFileName(fileName)));

            // register sending the file names to blob storage to be appended as a list
            Telegraph.Instance.Register<Messages.AppendNameToFileMessage, SendStringToAppendBlobStorage>(
                () => new Telegraphy.Azure.SendStringToAppendBlobStorage(
                    StorageConnectionString,
                    StorageContainerName,
                    true,
                    () => "Mp3List.txt"));

            Telegraph.Instance.Register<Messages.DeleteAllBlobsMsg, DeleteAllBlobsInContainer>(
                () => new Telegraphy.Azure.DeleteAllBlobsInContainer(StorageConnectionString));

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
                        StorageConnectionString,
                        TableStorageName,
                        TableOperationType.InsertOrReplace));

            Telegraph.Instance.Register<RetrieveFromTableStorageMessage, RetrieveFromTableStorage<string>>(
                    () => new Telegraphy.Azure.RetrieveFromTableStorage<string>(
                        StorageConnectionString,
                        TableStorageName));

            Telegraph.Instance.Register<DeleteFromTableStorageMessage, DeleteFromTableStorage>(
                    () => new Telegraphy.Azure.DeleteFromTableStorage(
                        StorageConnectionString,
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
