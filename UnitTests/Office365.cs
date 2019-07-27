using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Office365;

namespace UnitTests.Office365
{
    [TestClass]
    public class EmailTests
    {
        static string emailAccount;
        static string accountPassword;

        [TestMethod]
        public void TestGatheringUnreadEmailsFromInbox()
        {
            Telegraph.Instance.UnRegisterAll();

            bool receivedAnUnreadEmailMessage = false;
            int count = 0;
            Telegraph.Instance.Register(
                new OutlookInboxSubscriptionOperator(emailAccount, accountPassword, true),
                (EmailMessage msg) =>
                {
                    ++count;
                    receivedAnUnreadEmailMessage = true;
                });

            System.Threading.Thread.Sleep(10000); // wait 10 seconds for the emails to be polled.
            Assert.IsTrue(receivedAnUnreadEmailMessage);
        }
    }
}
