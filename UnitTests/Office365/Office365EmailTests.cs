﻿using Microsoft.Exchange.WebServices.Data;
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
        static string emailAccount = Connections.EmailAccount;
        static string accountPassword = Connections.EmailAccountPassword;

        [TestMethod]
        public void TestGatheringUnreadEmailsFromInbox()
        {
            Telegraph.Instance.UnRegisterAll();

            bool receivedAnUnreadEmailMessage = false;
            int count = 0;
            Telegraph.Instance.Register(
                new OutlookInboxSubscriptionOperator(emailAccount, accountPassword, true),
                (RecieveEmailMessage msg) =>
                {
                    ++count;
                    receivedAnUnreadEmailMessage = true;
                });

            System.Threading.Thread.Sleep(10000); // wait 10 seconds for the emails to be polled.
            Assert.IsTrue(receivedAnUnreadEmailMessage);
        }

        [TestMethod]
        public void TestSendingEmail()
        {
            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.Register<SendEmailMessage, SendEmail>(() => new SendEmail(emailAccount, accountPassword, "Test Friendly Name"));

            SendEmailMessage msg = new SendEmailMessage();
            msg.Subject = "Testing email for actor message";
            msg.ToEmailAddress = emailAccount;

            var askTask = Telegraph.Instance.Ask(msg);
            if (!askTask.Wait(new TimeSpan(0, 0, 45)))
                Assert.Fail("Waited too long to send a message");

            Assert.IsTrue(askTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion);

            bool found = DoesEmailExistInInbox(msg);

            Assert.IsTrue(found, "We did not find the item in the inbox");
        }

        [TestMethod]
        public void TestSendingEmailViaOperator()
        {
            Telegraph.Instance.UnRegisterAll();

            DateTime sent = DateTime.Now;
            long emailOperatorId = Telegraph.Instance.Register(new OutlookEmailPublisherOperator(emailAccount, accountPassword));
            Telegraph.Instance.Register<SendEmailMessage>(emailOperatorId);

            SendEmailMessage msg = new SendEmailMessage();
            msg.Subject = "Testing email for operator sent message";
            msg.ToEmailAddress = emailAccount;

            if (!Telegraph.Instance.Ask(msg).Wait(new TimeSpan(0, 0, 45)))
                Assert.Fail("Waited too long to send a message");

            bool found = DoesEmailExistInInbox(msg);

            Assert.IsTrue(found, "We did not find the item in the inbox");
        }

        private static bool DoesEmailExistInInbox(EmailMsg msg)
        {
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2016);
            service.Credentials = new WebCredentials(emailAccount, accountPassword);
            service.Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
            SearchFilter searchFilter = new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.DateTimeSent, msg.DateTimeSent);
            ItemView view = new ItemView(10);

            var emails = service.FindItems(WellKnownFolderName.Inbox, searchFilter, view);

            bool found = true;
            foreach (var item in emails)
            {
                if (item.Subject.Equals(msg.Subject))
                {
                    found = true;
                    item.Delete(DeleteMode.MoveToDeletedItems);
                    break;
                }
            }

            return found;
        }
    }
}
