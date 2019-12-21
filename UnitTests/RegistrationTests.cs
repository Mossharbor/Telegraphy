using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telegraphy.Net;
using UnitTests.TestImplemenations;

namespace UnitTests.TelegraphTests
{
    [TestClass]
    public class RegistrationTests
    {
        [TestMethod]
        public void RegisterOperatorOnly()
        {
            Telegraph.Instance.UnRegisterAll();
            LocalQueueOperator op = new LocalQueueOperator();
            long operatorID = Telegraph.Instance.Register(op);

            string message = "RegisterOperatorOnly";
            Telegraph.Instance.Tell(message);

            // We dont have an actor registered so we will this message will never be processed
            // however it should never throw an exception
        }

        [TestMethod]
        public void RegisterTypeAndOperator()
        {
            Telegraph.Instance.UnRegisterAll();
            LocalQueueOperator op = new LocalQueueOperator(new TestSwitchBoard());
            long operatorID = Telegraph.Instance.Register<string>(op);

            string message = "RegisterTypeAndOperator";
            Telegraph.Instance.Tell(message);

            // We dont have an actor registered so we will this message will never be processed
            // however it should never throw an exception with the test switch board
        }

        [TestMethod]
        public void RegisterTypeAndOperatorByOpID()
        {
            Telegraph.Instance.UnRegisterAll();
            LocalQueueOperator op = new LocalQueueOperator(new TestSwitchBoard());
            long operatorID = Telegraph.Instance.Register(op);
            Telegraph.Instance.Register<string>(operatorID);

            string message = "RegisterTypeAndOperatorByOpID";
            Telegraph.Instance.Tell(message);

            // We dont have an actor registered so we will this message will never be processed
            // however it should never throw an exception with the test switch board
        }

        [TestMethod]
        public void RegisterTypeActionAndOperatorByOpID()
        {
            Telegraph.Instance.UnRegisterAll();
            string message = "RegisterTypeActionAndOperatorByOpID";
            bool called = false;
            LocalQueueOperator op = new LocalQueueOperator();
            long operatorID = Telegraph.Instance.Register(op);
            Telegraph.Instance.Register<string>(operatorID, (data) =>
            {
                Assert.IsTrue(data.Equals(message));
                called = true;
            });

            Telegraph.Instance.Ask(message).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterMessageToActorByOp()
        {
            Telegraph.Instance.UnRegisterAll();
            string message = "RegisterMessageToActorByOp";
            bool called = false;
            LocalQueueOperator op = new LocalQueueOperator();
            DefaultActor da = new DefaultActor();
            Telegraph.Instance.Register<string, DefaultActor>(op, () => da);

            da.OnMessageHandler = (msg) => 
            {
                called = true;
                Assert.IsTrue(msg.Message.Equals(message));
                return true;
            };

            Telegraph.Instance.Ask(message.ToActorMessage()).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterMessageToActorByOpId()
        {
            Telegraph.Instance.UnRegisterAll();
            string message = "RegisterMessageToActorByOpId";
            bool called = false;
            LocalQueueOperator op = new LocalQueueOperator();
            long operatorID = Telegraph.Instance.Register(op);
            DefaultActor da = new DefaultActor();
            Telegraph.Instance.Register<string, DefaultActor>(operatorID, () => da);

            da.OnMessageHandler = (msg) =>
            {
                called = true;
                Assert.IsFalse(!msg.Message.Equals(message));
                return true;
            };

            Telegraph.Instance.Ask(message.ToActorMessage()).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterMessageToActor()
        {
            try
            {
                Telegraph.Instance.UnRegisterAll();
                var message = new TestQuestionListMessage(new string[] { "First Question", "Second Quesion" }, 12345, "BEEFFACE");
                bool called = false;
                TestQuestionStorage storage = new TestQuestionStorage();
                TestQuestionListExtractor extractor = new TestQuestionListExtractor();

                Telegraph.Instance.Register<TestQuestionListMessage, TestQuestionListExtractor>(() => new TestQuestionListExtractor());
                Telegraph.Instance.Register<TestQuestion, IQuestionStorage>(() => storage);
                Telegraph.Instance.Register<TestFirstQuestion, IQuestionStorage>(() => storage);

                Telegraph.Instance.Ask(message).Wait();
                Assert.IsTrue(TestQuestionListExtractor.msgRecievedCount == 1);
                Assert.IsTrue(TestQuestionListExtractor.msgSentCount == 2);
                Assert.IsTrue(TestQuestionStorage.msgRecievedCount == 2);
            }
            finally
            {
                TestQuestionListExtractor.msgRecievedCount = 0;
                TestQuestionListExtractor.msgSentCount = 0;
                TestQuestionStorage.msgRecievedCount = 0;
            }
        }

        [TestMethod]
        public void RegisterValueTypeToActor()
        {
            Telegraph.Instance.UnRegisterAll();
            int message = 1;
            bool called = false;
            LocalQueueOperator op = new LocalQueueOperator();
            long operatorID = Telegraph.Instance.Register(op);
            DefaultActor da = new DefaultActor();
            Telegraph.Instance.Register<ValueTypeMessage<int>, DefaultActor>(operatorID, () => da);

            da.OnMessageHandler = (msg) =>
            {
                called = true;
                Assert.IsFalse(!msg.Message.Equals(message));
                return true;
            };

            Telegraph.Instance.Ask(message.ToActorMessage()).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterValueTypeToActorForSingleThreadPerMessageTypeSwitchBoard()
        {
            Telegraph.Instance.UnRegisterAll();
            long computeOpID = Telegraph.Instance.Register(new LocalQueueOperator(new SingleThreadPerMessageTypeSwitchBoard()));
            int message = 1;
            bool called = false;
            DefaultActor da = new DefaultActor();
            Telegraph.Instance.Register<ValueTypeMessage<int>, DefaultActor>(computeOpID, () => da);

            da.OnMessageHandler = (msg) =>
            {
                called = true;
                Assert.IsFalse(!msg.Message.Equals(message));
                return true;
            };

            Telegraph.Instance.Ask(message.ToActorMessage()).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterValueTypeToActorForOneThreadPerActorTypeSwitchboard()
        {
            Telegraph.Instance.UnRegisterAll();
            long computeOpID = Telegraph.Instance.Register(new LocalQueueOperator(new OneThreadPerActorTypeSwitchboard()));
            int message = 1;
            bool called = false;
            DefaultActor da = new DefaultActor();
            Telegraph.Instance.Register<ValueTypeMessage<int>, DefaultActor>(computeOpID, () => da);

            da.OnMessageHandler = (msg) =>
            {
                called = true;
                Assert.IsFalse(!msg.Message.Equals(message));
                return true;
            };

            Telegraph.Instance.Ask(message.ToActorMessage()).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterValueArrayTypeToActorForOneThreadPerActorTypeSwitchboard()
        {
            Telegraph.Instance.UnRegisterAll();
            long computeOpID = Telegraph.Instance.Register(new LocalQueueOperator(new OneThreadPerActorTypeSwitchboard()));
            int[] message = new int[] { 1 };
            bool called = false;
            DefaultActor da = new DefaultActor();
            Telegraph.Instance.Register<ValueArrayTypeMessage<int>, DefaultActor>(computeOpID, () => da);

            da.OnMessageHandler = (msg) =>
            {
                called = true;
                Assert.IsFalse(!msg.Message.Equals(message));
                return true;
            };

            Telegraph.Instance.Ask(message.ToActorMessage()).Wait();
            Assert.IsTrue(called);
        }

        [TestMethod]
        public void RegisterValueTypeToAction()
        {
            Telegraph.Instance.UnRegisterAll();
            int msgCount = 0;
            int expectedMessageCount = 100;
            Telegraph.Instance.Register<ValueTypeMessage<int>>(count => 
            {
                Console.Write((int)count.Message + ",");
                ++msgCount;
            });

            List<Task<IActorMessage>> msgsToWaitOn = new List<Task<IActorMessage>>();

            for (int i = 0; i < expectedMessageCount; ++i)
            {
                // this should be sequential since we are on one thread for ints
                msgsToWaitOn.Add(Telegraph.Instance.Ask(i.ToActorMessage()));
            }

            Task.WaitAll(msgsToWaitOn.ToArray(), TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(msgCount == expectedMessageCount, "We did not get the correct number of messages");
        }

        [TestMethod]
        public void RegisterValueArrayTypeToAction()
        {
            Telegraph.Instance.UnRegisterAll();
            int msgCount = 0;
            int expectedMessageCount = 100;
            Telegraph.Instance.Register<ValueArrayTypeMessage<int>>(count =>
            {
                Console.Write((int[])count.Message + ",");
                ++msgCount;
            });

            List<Task<IActorMessage>> msgsToWaitOn = new List<Task<IActorMessage>>();

            for (int i = 0; i < expectedMessageCount; ++i)
            {
                int[] foo = new int[] { 1, 2, 3 };

                // this should be sequential since we are on one thread for ints
                msgsToWaitOn.Add(Telegraph.Instance.Ask(foo.ToActorMessage()));
            }

            Task.WaitAll(msgsToWaitOn.ToArray(), TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(msgCount == expectedMessageCount, "We did not get the correct number of messages");
        }

        [TestMethod]
        public void RegisterValueTypeToActionForSingleThreadPerMessageTypeSwitchBoard()
        {
            Telegraph.Instance.UnRegisterAll();
            long computeOpID = Telegraph.Instance.Register(new LocalQueueOperator(new SingleThreadPerMessageTypeSwitchBoard()));
            int msgCount = 0;
            int expectedMessageCount = 100;
            Telegraph.Instance.Register<ValueTypeMessage<int>>(computeOpID, count =>
            {
                Console.Write((int)count.Message + ",");
                ++msgCount;
            });

            List<Task<IActorMessage>> msgsToWaitOn = new List<Task<IActorMessage>>();

            for (int i = 0; i < expectedMessageCount; ++i)
            {
                // this should be sequential since we are on one thread for ints
                msgsToWaitOn.Add(Telegraph.Instance.Ask(i.ToActorMessage()));
            }

            Task.WaitAll(msgsToWaitOn.ToArray(), TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(msgCount == expectedMessageCount, "We did not get the correct number of messages");
        }
    }
}
