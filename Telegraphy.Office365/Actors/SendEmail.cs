using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Office365
{
    public class SendEmail : IActor
    {
        private string emailAddress;
        private string password;
        private string fromFriendlyName;

        public SendEmail(string emailAddress, string password, string fromFiendlyName)
        {
            this.emailAddress = emailAddress;
            this.password = password;
            this.fromFriendlyName = fromFiendlyName;
        }

        public virtual bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            if (!(msg is EmailMsg))
                throw new UnsupportedMessageException("Sending emails can only be done with a EmailMsg type. They do not support " + msg.GetType());

            OutlookEmailBaseOperator.SendEmail((msg as EmailMsg), this.emailAddress, this.password, this.fromFriendlyName);

            return true;
        }
    }
}
