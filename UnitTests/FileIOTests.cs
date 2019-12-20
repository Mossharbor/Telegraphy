using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.FileIO
{
    using Telegraphy.Net;
    using System.IO;
    using Telegraphy.File.IO;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;

    [TestClass]
    public class File
    {
        private readonly string queueRootPath = System.IO.Path.GetTempPath();

        private DirectoryQueue GetDirectoryQueue(string queueName)
        {
            DirectoryQueue queue = new DirectoryQueue(queueRootPath, queueName);
            return queue;
        }

        #region Directory Queue
        [TestMethod]
        public void SendActorMessageToDirectoryQueue()
        {
            string queueName = "test-" + "SendActorMessageToStorageQueue".ToLower();
            using (var queue = GetDirectoryQueue(queueName))
            {
                try
                {
                    string message = "HelloWorld";
                    PingPong.Ping aMsg = new PingPong.Ping(message);
                    IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                    var sendMessageQueue = new SendMessageToDirectoryQueue(queueRootPath, queueName, true);
                    Telegraph.Instance.Register<PingPong.Ping, SendMessageToDirectoryQueue>(() => sendMessageQueue);
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
                    queue.Delete();
                    GetDirectoryQueue(queueName + "-deadletter").Delete();
                }
            }
        }

        [TestMethod]
        public void RecieveActorMessageFromDirectoryQueue()
        {
            string queueName = "test-" + "RecieveActorMessageFromStorageQueue".ToLower();
            using (var queue = GetDirectoryQueue(queueName))
            {
                queue.CreateIfNotExists();
                try
                {
                    string message = "HelloWorld";
                    var actorMessage = new PingPong.Ping(message);
                    SerializeMessage<IActorMessage> sMsg = new SerializeMessage<IActorMessage>(actorMessage);
                    IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
                    serializer.OnMessageRecieved(sMsg);
                    byte[] msgBytes = (byte[])sMsg.ProcessingResult;
                    ManualResetEvent received = new ManualResetEvent(false);

                    IActorMessageDeserializationActor deserilaizationActor = new IActorMessageDeserializationActor();
                    Telegraph.Instance.Register<DeserializeMessage<IActorMessage>>(deserilaizationActor);
                    deserilaizationActor.Register((object msg) => (IActorMessage)msg);
                    deserilaizationActor.Register((object msg) => (PingPong.Ping)msg);

                    long azureOperatorID = Telegraph.Instance.Register(
                        new DirectoryQueueSubscriptionOperator<IActorMessage>(LocalConcurrencyType.DedicatedThreadCount, queueRootPath, queueName, false, 2),
                        (PingPong.Ping foo) => 
                        {
                            Assert.IsTrue(message.Equals((string)foo.Message, StringComparison.InvariantCulture));
                            received.Set();
                        });

                    queue.AddMessage(new DirectoryQueueMessage(msgBytes));

                    Assert.IsTrue(received.WaitOne(TimeSpan.FromSeconds(13), true), "We did not receive the message");
                }
                finally
                {
                    Telegraph.Instance.UnRegisterAll();
                    GetDirectoryQueue(queueName).Delete();
                    GetDirectoryQueue(queueName + "-deadletter").Delete();
                }
            }
        }

        [TestMethod]
        public void SendStringToDirectoryQueue()
        {
            string queueName = "test-" + "SendStringStorageQueue".ToLower();
            using (var queue = GetDirectoryQueue(queueName))
            {
                try
                {
                    string message = "HelloWorld";
                    Telegraph.Instance.Register<string, SendStringToDirectoryQueue>(() => new SendStringToDirectoryQueue(queueRootPath, queueName, true));
                    Telegraph.Instance.Ask(message).Wait();
                    Assert.IsTrue(queue.GetMessage().AsString.Equals(message));
                }
                finally
                {
                    Telegraph.Instance.UnRegisterAll();
                    GetDirectoryQueue(queueName).Delete();
                    GetDirectoryQueue(queueName + "-deadletter").Delete();
                }
            }
        }

        [TestMethod]
        public void SendStringToDirectoryueue()
        {
            string queueName = "test-" + "SendStringStorageQueue".ToLower();
            var queue = GetDirectoryQueue(queueName);
            try
            {
                string message = "HelloWorld";
                Telegraph.Instance.Register<string, SendStringToDirectoryQueue>(() => new SendStringToDirectoryQueue(queueRootPath, queueName, true));
                Telegraph.Instance.Ask(message).Wait();
                Assert.IsTrue(queue.GetMessage().AsString.Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetDirectoryQueue(queueName).Delete();
                GetDirectoryQueue(queueName + "-deadletter").Delete();
            }
        }

        [TestMethod]
        public void RecieveStringFromDirectoryQueue()
        {
            string queueName = "test-" + "RecieveStringStorageQueue".ToLower();
            var queue = GetDirectoryQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                queue.AddMessage(new DirectoryQueueMessage(message));

                long azureOperatorID = Telegraph.Instance.Register(
                    new DirectoryQueueSubscriptionOperator<string>(LocalConcurrencyType.DedicatedThreadCount, queueRootPath, queueName, false, 2),
                    (string foo) => { Assert.IsTrue(message.Equals(foo, StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetDirectoryQueue(queueName).Delete();
                GetDirectoryQueue(queueName + "-deadletter").Delete();
            }
        }

        [TestMethod]
        public void SendBytesToDirectoryQueue()
        {
            string queueName = "test-" + "SendBytesToDirectoryQueue".ToLower();
            var queue = GetDirectoryQueue(queueName);
            try
            {
                string message = "HelloWorld";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                Telegraph.Instance.Register<byte[], SendBytesToDirectoryQueue>(() => new SendBytesToDirectoryQueue(queueRootPath, queueName, true));
                Telegraph.Instance.Ask(messageBytes.ToActorMessage()).Wait();
                Assert.IsTrue(Encoding.UTF8.GetString(queue.GetMessage().AsBytes).Equals(message));
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetDirectoryQueue(queueName).Delete();
                GetDirectoryQueue(queueName + "-deadletter").Delete();
            }
        }

        [TestMethod]
        public void RecieveBytesFromDirectoryQueue()
        {
            string queueName = "test-" + "RecieveBytesFromDirectoryQueue".ToLower();
            var queue = GetDirectoryQueue(queueName);
            queue.CreateIfNotExists();
            try
            {
                string message = "HelloWorld";
                queue.AddMessage(new DirectoryQueueMessage(message));

                long azureOperatorID = Telegraph.Instance.Register(
                    new DirectoryQueueSubscriptionOperator<byte[]>(LocalConcurrencyType.DedicatedThreadCount, queueRootPath, queueName, false, 2),
                    (ValueTypeMessage<byte> foo) => { Assert.IsTrue(message.Equals(Encoding.UTF8.GetString((byte[])foo.Message), StringComparison.InvariantCulture)); });
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
                GetDirectoryQueue(queueName).Delete();
                GetDirectoryQueue(queueName + "-deadletter").Delete();
            }
        }
        #endregion

        public string GetPathToTestFile(string subDir = null)
        {
            string file = System.IO.Path.GetTempFileName();
            if (null == subDir)
                return file;

            string tempFileName = Path.GetFileName(file);
            string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), subDir);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string finalPath = Path.Combine(dir, tempFileName);
            System.IO.File.Move(file, finalPath);
            return finalPath;
        }

        public void TestSendStreamToStorage(Action setupFunction, string fileToSend, string destinationFile)
        {
            try
            {
                setupFunction();

                Telegraph.Instance.Ask((System.IO.Stream)System.IO.File.OpenRead(fileToSend)).Wait();

                Assert.IsTrue(System.IO.File.Exists(destinationFile));
                System.IO.File.Delete(destinationFile);
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
            }

        }

        public void TestSendBytesToFile(Action setupFunction, string destinationFile, byte[] data = null)
        {
            try
            {
                string stringToSend = "Foobar";
                byte[] msgBytes = (null == data) ? Encoding.UTF8.GetBytes(stringToSend) : data;

                setupFunction();

                Telegraph.Instance.Ask(msgBytes.ToActorMessage()).Wait();

                Assert.IsTrue(System.IO.File.Exists(destinationFile));
                using (StreamReader sr = new StreamReader(destinationFile))
                {
                    string contents = sr.ReadToEnd();
                    Assert.IsTrue(stringToSend == contents);
                }
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        public void TestSendStringToFile(Action setupFunction, string stringToSend, string destinationFile)
        {
            try
            {
                int index = 0;
                setupFunction();

                Telegraph.Instance.Ask(stringToSend).Wait();

                Assert.IsTrue(System.IO.File.Exists(destinationFile));
                using (StreamReader sr = new StreamReader(destinationFile))
                {
                    string contents = sr.ReadToEnd();
                    Assert.IsTrue(stringToSend == contents);
                }
            }
            finally
            {
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void SendStreamToFile()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string fileToSend = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();
            string destinationFile = this.GetPathToTestFile();

            TestSendStreamToStorage(() =>
            {
                Telegraph.Instance.Register<System.IO.Stream, SendStreamToFile>(
                () => new Telegraphy.File.IO.SendStreamToFile(destinationFile));
            }
            , fileToSend
            , destinationFile);
        }

        [TestMethod]
        public void SendStreamToAppendToFile()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string fileToSend = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").First();
            string destinationFile = this.GetPathToTestFile();

            TestSendStreamToStorage(() =>
            {
                Telegraph.Instance.Register<System.IO.Stream, SendStreamToAppendToFile>(
                () => new Telegraphy.File.IO.SendStreamToAppendToFile(destinationFile));
            }
            , fileToSend
            , destinationFile);
        }

        [TestMethod]
        public void SendStringToAppendToFile()
        {
            string stringToSend = "Foobar";
            string destinationFile = this.GetPathToTestFile();

            TestSendStringToFile(() =>
            {
                Telegraph.Instance.Register<string, SendStringToAppendToFile>(
                    () => new Telegraphy.File.IO.SendStringToAppendToFile(destinationFile));
            }
            , stringToSend
            , destinationFile);
        }

        [TestMethod]
        public void SendBytesToAppendToFile()
        {
            string stringToSend = "Foobar";
            byte[] bytesToSend = System.Text.ASCIIEncoding.ASCII.GetBytes(stringToSend);
            string destinationFile = this.GetPathToTestFile();

            TestSendBytesToFile(() =>
            {
                Telegraph.Instance.Register<byte[], SendBytesToAppendToFile>(() => new SendBytesToAppendToFile(destinationFile));
            }
            , destinationFile
            , bytesToSend);
        }

        [TestMethod]
        public void SendStreamToTruncateFile()
        {
            string destinationFile = this.GetPathToTestFile();

            try
            {
                using (StreamWriter sw = new StreamWriter(destinationFile))
                {
                    sw.WriteLine("this line should be truncated");
                }

                Telegraph.Instance.Register<System.IO.MemoryStream, SendStreamToTruncateFile>(
                () => new Telegraphy.File.IO.SendStreamToTruncateFile(destinationFile));

                using (StreamWriter sw = new StreamWriter(destinationFile))
                {
                    sw.WriteLine("this line should also be truncated");
                }

                string stringToSend = "";
                for (int i = 0; i < 512; ++i)
                    stringToSend += 'a';

                byte[] buffer = Encoding.UTF8.GetBytes(stringToSend);
                Telegraph.Instance.Ask(new System.IO.MemoryStream(buffer)).Wait();

                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                FileInfo fInfo = new FileInfo(destinationFile);
                Assert.IsTrue(fInfo.Length == buffer.Length);
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void SendStringToTruncateFile()
        {
            string destinationFile = this.GetPathToTestFile();

            try
            {
                using (StreamWriter sw = new StreamWriter(destinationFile))
                {
                    sw.WriteLine("this line should be truncated");
                }

                Telegraph.Instance.Register<string, SendStringToTruncateFile>(
                () => new Telegraphy.File.IO.SendStringToTruncateFile(destinationFile));

                using (StreamWriter sw = new StreamWriter(destinationFile))
                {
                    sw.WriteLine("this line should also be truncated");
                }

                string stringToSend = "";
                for (int i = 0; i < 512; ++i)
                    stringToSend += 'a';

                Telegraph.Instance.Ask(stringToSend).Wait();

                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                FileInfo fInfo = new FileInfo(destinationFile);
                Assert.IsTrue(fInfo.Length == stringToSend.Length);
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void SendBytesToTruncateFile()
        {
            string destinationFile = this.GetPathToTestFile();

            try
            {
                using (StreamWriter sw = new StreamWriter(destinationFile))
                {
                    sw.WriteLine("this line should be truncated");
                }

                Telegraph.Instance.Register<byte[], SendBytesToTruncateFile>(
                () => new Telegraphy.File.IO.SendBytesToTruncateFile(destinationFile));

                using (StreamWriter sw = new StreamWriter(destinationFile))
                {
                    sw.WriteLine("this line should also be truncated");
                }

                string stringToSend = "";
                for (int i = 0; i < 512; ++i)
                    stringToSend += 'a';

                byte[] buffer = Encoding.UTF8.GetBytes(stringToSend);
                Telegraph.Instance.Ask(buffer.ToActorMessage()).Wait();

                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                FileInfo fInfo = new FileInfo(destinationFile);
                Assert.IsTrue(fInfo.Length == buffer.Length);
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void ReceivedFileAsBytes()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, RecieveBytesFromFile>(
                   () => new Telegraphy.File.IO.RecieveBytesFromFile(destFolder));

                byte[] sentBytes = (byte[])(Telegraph.Instance.Ask(destFilename).Result).ProcessingResult;
                string sentString = Encoding.UTF8.GetString(sentBytes);

                Assert.IsTrue(sentString.Equals(stringToSend));
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void ReceivedFileAsString()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, RecieveFileAsString>(
                   () => new Telegraphy.File.IO.RecieveFileAsString(destFolder));

                string sentString = (string)(Telegraph.Instance.Ask(destFilename).Result).ProcessingResult;

                Assert.IsTrue(sentString.Equals(stringToSend));
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void ReceivedFileAsStream()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, RecieveFileAsStream>(
                   () => new Telegraphy.File.IO.RecieveFileAsStream(destFolder));

                Stream sentBytes = (Stream)(Telegraph.Instance.Ask(destFilename).Result).ProcessingResult;
                string sentString = string.Empty;
                using (StreamReader sw = new StreamReader(sentBytes))
                {
                    sentString = sw.ReadToEnd();
                }

                Assert.IsTrue(!string.IsNullOrEmpty(sentString));
                Assert.IsTrue(sentString.Equals(stringToSend));
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void ReceivedFileAsStringArray()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, RecieveFileAsStringArray>(
                   () => new Telegraphy.File.IO.RecieveFileAsStringArray(destFolder));

                string[] sentStrings = (string[])(Telegraph.Instance.Ask(destFilename).Result).ProcessingResult;

                Assert.IsTrue(sentStrings.Length == 1);

                string sentString = sentStrings[0];

                Assert.IsTrue(!string.IsNullOrEmpty(sentString));
                Assert.IsTrue(sentString.Equals(stringToSend));
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void RecieveFileInfo()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, RecieveFileInfo>(
                   () => new Telegraphy.File.IO.RecieveFileInfo(destFolder));

                FileInfo fInfo = (FileInfo)(Telegraph.Instance.Ask(destFilename).Result).ProcessingResult;

                Assert.IsTrue(null != fInfo);
            }
            finally
            {
                System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void DeleteFile()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, DeleteFile>(
                   () => new Telegraphy.File.IO.DeleteFile(destFolder));

                Telegraph.Instance.Ask(destFilename).Wait();

                Assert.IsTrue(!System.IO.File.Exists(destFilename));
            }
            finally
            {
                if (System.IO.File.Exists(destinationFile))
                    System.IO.File.Delete(destinationFile);
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void DeleteFile2()
        {
            string destinationFile = this.GetPathToTestFile();
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, DeleteFile>(
                   () => new Telegraphy.File.IO.DeleteFile());

                Telegraph.Instance.Ask(destinationFile).Wait();

                Assert.IsTrue(!System.IO.File.Exists(destFilename));
            }
            finally
            {
                if (System.IO.File.Exists(destinationFile))
                    System.IO.File.Delete(destinationFile);

                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void DeleteAllFilesInFolder()
        {
            string destinationFile = this.GetPathToTestFile("subDir");
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, DeleteAllFilesInFolder>(
                   () => new Telegraphy.File.IO.DeleteAllFilesInFolder());

                Telegraph.Instance.Ask(destFolder).Wait();

                Assert.IsTrue(!System.IO.File.Exists(destinationFile));
                Assert.IsTrue(System.IO.Directory.Exists(destFolder));
            }
            finally
            {
                if (System.IO.File.Exists(destinationFile))
                    System.IO.File.Delete(destinationFile);

                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationFile)))
                    System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(destinationFile));

                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void DeleteFolder()
        {
            string destinationFile = this.GetPathToTestFile("subDir");
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(destinationFile));
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, DeleteFolder>(
                   () => new Telegraphy.File.IO.DeleteFolder(destFolder));

                Telegraph.Instance.Ask("subDir").Wait();

                Assert.IsTrue(!System.IO.File.Exists(destinationFile));
                Assert.IsTrue(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationFile)));
            }
            finally
            {
                if (System.IO.File.Exists(destinationFile))
                    System.IO.File.Delete(destinationFile);

                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationFile)))
                    System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(destinationFile));

                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void DeleteFolder2()
        {
            string destinationFile = this.GetPathToTestFile("subDir");
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string stringToSend = "foobar";

            try
            {
                Assert.IsTrue(System.IO.File.Exists(destinationFile));

                System.IO.File.WriteAllText(destinationFile, stringToSend);

                Telegraph.Instance.UnRegisterAll();

                Telegraph.Instance.Register<string, DeleteFolder>(
                   () => new Telegraphy.File.IO.DeleteFolder());

                Telegraph.Instance.Ask(destFolder).Wait();

                Assert.IsTrue(!System.IO.File.Exists(destinationFile));
                Assert.IsTrue(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationFile)));
            }
            finally
            {
                if (System.IO.File.Exists(destinationFile))
                    System.IO.File.Delete(destinationFile);
                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationFile)))
                    System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(destinationFile));
                Telegraph.Instance.UnRegisterAll();
            }
        }

        [TestMethod]
        public void TestFilePipeline()
        {
            string localMusicFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            IEnumerable<string> files = System.IO.Directory.GetFiles(localMusicFolder, "*.mp3").Take(2);

            string destinationFile = this.GetPathToTestFile("subDir");
            string destFilename = System.IO.Path.GetFileName(destinationFile);
            string destFolder = System.IO.Path.GetDirectoryName(destinationFile);
            string fileList = System.IO.Path.Combine(destFolder, "Mp3List.txt");

            try
            {
                System.IO.File.Delete(destinationFile);

                // register sending the file themselves to blob storage
                CopyFile sendFileToDest = new CopyFile(destFolder, true);
                Telegraph.Instance.Register<Messages.UploadMessage>(sendFileToDest);

                // register sending the file names to blob storage to be appended as a list
                SendStringToAppendToFile mp3List = new SendStringToAppendToFile(fileList);
                Telegraph.Instance.Register<Messages.AppendNameToFileMessage>(mp3List);

                DeleteAllFilesInFolder clearOutAllFiles = new DeleteAllFilesInFolder();
                Telegraph.Instance.Register<Messages.DeleteAllFilesMsg>(clearOutAllFiles);

                var pipeline = Telegraphy.Net.Pipeline
                    // We are creating a pipeline that takes in a string (filename) and returns an UploadMessage
                    .Create<string, Messages.UploadMessage>((string fileName) =>
                    {
                        return new Messages.UploadMessage(fileName);
                    })
                    // We are sending that upload message for the file to be copied
                    .Next<string>((Messages.UploadMessage msg) =>
                    {
                        Telegraph.Instance.Ask(msg).Wait();
                        return (string)msg.Message;
                    })
                    // we are sending name of the file we uploaded returns an upload message
                    .Next<Messages.AppendNameToFileMessage>((string fileName) =>
                    {
                        return new Messages.AppendNameToFileMessage(fileName);
                    })
                    // We are creating a pipeline that takes in an UploadMessage (filename) appends it to the append file
                    .Next<string>((Messages.AppendNameToFileMessage msg) =>
                    {
                        Telegraph.Instance.Ask(msg).Wait();
                        return (string)msg.Message; // fileName
                    });

                // Process all fo the files in the list
                var filesNames = pipeline.Process(files).ToArray();

                Assert.IsTrue(System.IO.Directory.GetFiles(destFolder).Length == 3);
                string[] readList = System.IO.File.ReadAllLines(fileList);

                foreach (var t in files)
                {
                    Assert.IsTrue(Array.Exists(readList, p => p.Equals(t)), "missing file" + t);
                }
            }
            finally
            {
                Telegraph.Instance.Ask(new Messages.DeleteAllFilesMsg(destFolder)).Wait();
                Telegraph.Instance.UnRegisterAll();

                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(destinationFile)))
                    System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(destinationFile));
            }
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

            public class DeleteAllFilesMsg : SimpleMessage<DeleteAllFilesMsg>
            {
                public DeleteAllFilesMsg(string container) { this.Message = container; }
            }
        }
    }
}
