using Microsoft.Exchange.WebServices.Data;
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
        static string emailAccount = null;
        static string accountPassword = null;

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

        [TestMethod]
        public void TestSendingEmail()
        {
            Telegraph.Instance.UnRegisterAll();

            DateTime sent = DateTime.Now;
            long emailOperatorId = Telegraph.Instance.Register(new OutlookEmailPublisherOperator(emailAccount, accountPassword));
            Telegraph.Instance.Register<EmailMsg>(emailOperatorId);

            EmailMsg msg = new EmailMsg();
            msg.Subject = "Testing email";
            msg.ToEmailAddress = emailAccount;

            if (!Telegraph.Instance.Ask(msg).Wait(new TimeSpan(0, 0, 45)))
                Assert.Fail("Waited too long to send a message");
            
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

            Assert.IsTrue(found, "We did not find the item in the inbox");
        }
    }
}
