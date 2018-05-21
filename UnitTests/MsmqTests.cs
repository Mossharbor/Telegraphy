using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Msmq
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Messaging;
    using Telegraphy.Net;
    using System.IO;
    using Telegraphy.Msmq;

    [TestClass]
    public class MsmqTests
    {
        //NOTE: to run these tests you need to have the msmq windows feature installed.

        [TestMethod]
        public void TestSendingStringsToMsmq()
        {
            // create message
            string message = @"Hello World";
            string queueName = "TestSendingStringsToMsmq"+Guid.NewGuid().ToString().Substring(0,4);
            // Setup send to queue with an actor that uses lazy instatiation
            Telegraph.Instance.Register<string, MsmqDeliveryOperator<string>>(() => new MsmqDeliveryOperator<string>(queueName));

            // Send message to queue
            int i = 0;
            List<Task> waitTasks = new List<Task>();
            for (i = 0; i < 100; ++i)
            {
                waitTasks.Add(Telegraph.Instance.Ask(message));
            }

            Task.WaitAll(waitTasks.ToArray());
            Telegraph.Instance.UnRegisterAll();

            int count = 0;
            MsmqReceptionOperator<string> receptionOp = new MsmqReceptionOperator<string>(queueName);
            long azureOperatorID = Telegraph.Instance.Register(receptionOp);
            Telegraph.Instance.Register<string>(azureOperatorID, stringMsg =>
            {
                Assert.IsTrue(stringMsg.Equals(message));
                System.Threading.Thread.Sleep(100);
                System.Threading.Interlocked.Increment(ref count);
            });

            int attempts = 20;
            while (attempts != 0 && count < i)
            {
                System.Threading.Thread.Sleep(1000);
                --attempts;
            }
            System.Diagnostics.Debug.WriteLine("Count:" + count +" TotalSent:"+ MsmqBaseOperator<string>.TotalSendsCalled);
            Assert.IsTrue(count == i);
            System.Messaging.MessageQueue.Delete(receptionOp.Queue.Path);


        }

        [TestMethod]
        public void TestSendingByteArrayToMsmq()
        {
            // create message
            string message = @"Hello World";
            string queueName = "TestSendingByteArrayToMsmq";

            MsmqDeliveryOperator<byte[]> deliveryOp = new MsmqDeliveryOperator<byte[]>(queueName);
            Telegraph.Instance.Register<byte[], MsmqDeliveryOperator<byte[]>>(() => deliveryOp);

            // Send message to queue
            List<Task> waitTasks = new List<Task>();
            int i = 0;
            for (i = 0; i < 100; ++i)
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                waitTasks.Add(Telegraph.Instance.Ask(msgBytes));
            }

            Task.WaitAll(waitTasks.ToArray());
            Telegraph.Instance.UnRegisterAll();

            int count = 0;
            MsmqReceptionOperator<byte[]> receptionOp = new MsmqReceptionOperator<byte[]>(queueName);
            long azureOperatorID = Telegraph.Instance.Register(receptionOp);
            Telegraph.Instance.Register<byte[]>(azureOperatorID, bytemsg =>
            {
                string stringmsg = Encoding.UTF8.GetString(bytemsg);
                System.Threading.Thread.Sleep(100);
                System.Threading.Interlocked.Increment(ref count);
            });

            int attempts = 20;
            while (attempts != 0 && count < i)
            {
                System.Threading.Thread.Sleep(1000);
                --attempts;
            }
            System.Diagnostics.Debug.WriteLine("Count:" + count);
            Assert.IsTrue(count == i);
            System.Messaging.MessageQueue.Delete(receptionOp.Queue.Path);
        }

        [TestMethod]
        public void TestSendingObjectToMsmq()
        {
            // create message
            string message = @"Hello World";
            string queueName = "testQueue";

        }
    }
}
