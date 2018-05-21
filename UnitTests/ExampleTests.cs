
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Examples
{
    using Telegraphy.Net;

    [TestClass]
    public class ExampleTests
    {
        [TestMethod]
        public void HelloWorld()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.HelloWorld();
        }

        [TestMethod]
        public void HelloWorldWithLazyActorInstantiation()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.HelloWorldWithLazyActorInstantiation();
        }

        [TestMethod]
        public void SimpleSingleThreadSequential()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.SimpleSingleThreadSequential();
        }

        [TestMethod]
        public void ThreadPool()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.ThreadPool();
        }

        [TestMethod]
        public void LimitedThreadPool()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.LimitedThreadPool();
        }

        [TestMethod]
        public void WorkerThreads()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.WorkerThreads();
        }

        [TestMethod]
        public void LazyInstantiation()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.LazyInstantiation();
        }

        [TestMethod]
        public void LazyInstantiation2()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.LazyInstantiation2();
        }
        [TestMethod]
        public void WaitOnMultipleMessagesToComplete()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.WaitOnMultipleMessagesToComplete();
        }
        [TestMethod]
        public void WaitForCompletion()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.WaitForCompletion();
        }
        [TestMethod]
        public void MessageTimeOut()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.MessageTimeOut();
        }
        [TestMethod]
        public void MessageCancelled()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.MessageCancelled();
        }
        [TestMethod]
        public void GetResultOfProcessing()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.GetResultOfProcessing();
        }
        [TestMethod]
        public void MessageOrdering()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.MessageOrdering();
        }
        [TestMethod]
        public void MessageOrdering2()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.MessageOrdering2();
        }
        [TestMethod]
        public void BasicMessageSerializationDeserialization()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.BasicMessageSerializationDeserialization();
        }
        [TestMethod]
        public void ComplexMessageSerializationDeserialization()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.ComplexMessageSerializationDeserialization();
        }
        [TestMethod]
        public void MultipleOperatorsBasic()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.MultipleOperatorsBasic();
        }
        [TestMethod]
        public void BroadcastToAllOperators()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.BroadcastToAllOperators();
        }
        [TestMethod]
        public void ThrottlingIncomingMessages()
        {
            Telegraph.Instance.UnRegisterAll();
            BasicStartHere.P.ThrottlingIncomingMessages();
        }
    }
}
