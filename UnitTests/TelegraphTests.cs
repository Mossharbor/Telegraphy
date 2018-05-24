using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telegraphy.Net;

namespace UnitTests.TelegraphTests
{
    [TestClass]
    public class TelegraphTests
    {
        [TestMethod]
        public void RegisterOperatorOnly()
        {
            Telegraph.Instance.UnRegisterAll();
            LocalOperator op = new LocalOperator();
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
            LocalOperator op = new LocalOperator(new TestSwitchBoard());
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
            LocalOperator op = new LocalOperator(new TestSwitchBoard());
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
            LocalOperator op = new LocalOperator();
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
            LocalOperator op = new LocalOperator();
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
            LocalOperator op = new LocalOperator();
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
    }
}
