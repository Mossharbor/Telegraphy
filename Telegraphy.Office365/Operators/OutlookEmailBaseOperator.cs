using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Office365
{
    public abstract class OutlookEmailBaseOperator: IOperator
    {
        internal const int DefaultDequeueMaxCount = 1;
        internal const int DefaultConcurrency = 3;
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        bool recieveMessagesOnly = false;
        ControlMessages.HangUp hangUp = null;
        SearchFilter filter = null;
        WellKnownFolderName folder;
        SortDirection sortDirection;
        string emailAddress;
        string password;
        string fromFriendlyName; //TODO
        int maxDequeueCount;
        DateTime timeSinceLastServerQuery = new DateTime(0);
        TimeSpan throttleLimit = TimeSpan.FromMinutes(10);
        ConcurrentQueue<Item> emailQueue = new ConcurrentQueue<Item>();

        protected OutlookEmailBaseOperator(
               ILocalSwitchboard switchboard,
               string emailAddress,
               string password,
               int maxDequeueCount,
               SearchFilter filter = null,
               WellKnownFolderName folder = WellKnownFolderName.Inbox,
               SortDirection sortDirection = SortDirection.Ascending)
        {
            this.emailAddress = emailAddress;
            this.password = password;
            this.recieveMessagesOnly = true;
            this.filter = filter;
            this.folder = folder;
            this.maxDequeueCount = maxDequeueCount;
            this.sortDirection = sortDirection;

            if (null != switchboard)
            {
                this.Switchboards.Add(switchboard);
                switchboard.Operator = this;
            }

            this.ID = 0;

            if (null == switchboard && this.recieveMessagesOnly)
                throw new SwitchBoardNeededWhenRecievingMessagesException();
        }

        protected OutlookEmailBaseOperator(
            string emailAddress, 
            string password)
        {
            this.emailAddress = emailAddress;
            this.password = password;
            this.recieveMessagesOnly = false;

            this.ID = 0;
        }

        public long ID { get; set; }

        public ulong Count
        {
            get
            {
                return 0; // todo return the number of emails left to process.
            }
        }

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }

        public void AddMessage(IActorMessage msg)
        {
            if (msg is ControlMessages.HangUp)
            {
                if (!recieveMessagesOnly)
                {
                    if (null != msg.Status && !msg.Status.Task.IsCompleted)
                        msg.Status.SetResult(msg);
                    return;
                }

                hangUp = (msg as ControlMessages.HangUp);
                Kill();
                return;
            }

            if (recieveMessagesOnly)
                throw new Telegraphy.Net.OperatorCannotSendMessagesException();

            if (!(msg is EmailMsg))
                throw new UnsupportedMessageException("Outlook Email Operators only support the Email Message type. They do not support " + msg.GetType());


            try
            {
                SendEmail((EmailMsg)msg);

                if (null != msg.Status && !msg.Status.Task.IsCanceled)
                    msg.Status.TrySetResult(msg);

            }
            catch (Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);

                if (null != msg.Status && !msg.Status.Task.IsCanceled)
                    msg.Status.TrySetException(ex);
            }
        }

        private void SendEmail(EmailMsg emailMsg)
        {
            MailMessage msg = emailMsg.ToMailMessage(this.emailAddress, this.password, this.fromFriendlyName);

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(emailAddress, this.password);
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;

            emailMsg.DateTimeSent = DateTime.Now;

            client.Send(msg);
        }

        public IActorMessage GetMessage()
        {
            if (!recieveMessagesOnly)
                throw new OperatorCannotRecieveMessagesException();

            if (null != hangUp)
                return hangUp;

            try
            {
                IActorMessage msg = null;
                
                if (emailQueue.IsEmpty)
                {
                    if (DateTime.Now - timeSinceLastServerQuery < throttleLimit)
                        return null;

                    lock(emailQueue)
                    {
                        if (emailQueue.IsEmpty && null == hangUp)
                            QueueUpEmails();

                        if (null != hangUp)
                            return hangUp;
                    }
                }
                Item email = null;
                if (!emailQueue.TryDequeue(out email))
                    return null;

                msg = email.ToActorMessage(this.emailAddress);

                if (null == msg.Status)
                    msg.Status = new TaskCompletionSource<IActorMessage>();
                
                return msg;
            }
            catch (Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
                return null;
            }
        }

        public void Kill()
        {
            foreach (var switchBoard in this.Switchboards)
                switchBoard.Disable();
        }

        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;

            // TODO figure out how know when we hvae processed all of the emails

            return ((DateTime.Now - start) <= timeout);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            AddMessage(msg);
            return true;
        }

        private void QueueUpEmails()
        {
            // https://blog.sqltreeo.com/read-emails-from-exchange-online-mailbox-office-365-into-sql-server/
            // https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/get-started-with-ews-managed-api-client-applications
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2016);
            service.Credentials = new WebCredentials(this.emailAddress, this.password);
            // NOTE: this is for exchange and is slow service.AutodiscoverUrl(this.emailAddress, RedirectionUrlValidationCallback);
            service.Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");

            
            //FolderId inbox = new FolderId(WellKnownFolderName.Inbox, mailbox);
            //archFilter searchFilter = new SearchFilter.IsGreaterThan(ItemSchema.DateTimeSent, DateTime.Now);
            //SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false))
            //ItemView view = new ItemView(maxDequeueCount); // take 10 items
            //FindItemsResults<Item> emails = service.FindItems(inbox, searchFilter, view);

            FindItemsResults <Item> emails = null;
            ItemView view = new ItemView(maxDequeueCount);
            view.OrderBy.Add(ItemSchema.DateTimeReceived, this.sortDirection);

            if (null == this.filter)
            {
                emails = service.FindItems(this.folder, view);
            }
            else
            {
                emails = service.FindItems(this.folder, this.filter, view);
            }

            timeSinceLastServerQuery = DateTime.Now;

            if (emails.Any())
                service.LoadPropertiesForItems(emails, PropertySet.FirstClassProperties);

            foreach (var item in emails)
            {
                emailQueue.Enqueue(item);
            }
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);
            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }
    }
}