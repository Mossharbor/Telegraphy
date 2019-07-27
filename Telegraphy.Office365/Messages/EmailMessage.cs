using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Office365
{
    public class EmailMsg : IActorMessage
    {
        private string toEmailAddress;
        private string toEmailFriendlyName;
        private string subject;
        private string body;
        private bool isBodyHtml;
        private string fromEmailAddress;
        private DateTime? dateTimeSent;
        private DateTime? dateTimeRecieved;

        public object Message { get => subject; set => subject = value.ToString(); }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public string ToEmailAddress { get => toEmailAddress; set => toEmailAddress = value; }
        public string ToEmailFriendlyName { get => toEmailFriendlyName; set => toEmailFriendlyName = value; }
        public string Subject { get => subject; set => subject = value; }
        public string Body { get => body; set => body = value; }
        public bool IsBodyHtml { get => isBodyHtml; set => isBodyHtml = value; }
        public string FromEmailAddress { get => fromEmailAddress; set => fromEmailAddress = value; }
        public DateTime? DateTimeSent { get => dateTimeSent; set => dateTimeSent = value; }
        public DateTime? DateTimeRecieved { get => dateTimeRecieved; set => dateTimeRecieved = value; }

        internal MailMessage ToMailMessage(string fromEmailAddress, string password, string fromFriendlyName)
        {
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(this.ToEmailAddress, this.ToEmailFriendlyName));
            msg.From = new MailAddress(fromEmailAddress, fromFriendlyName); 
            msg.Subject = this.Subject;
            msg.Body = this.Body;
            msg.IsBodyHtml = this.IsBodyHtml;
            return msg;
        }
    }
}
