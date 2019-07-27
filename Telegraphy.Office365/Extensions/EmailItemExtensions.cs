using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Office365
{
    internal static class EmailItemExtensions
    {
        public static IActorMessage ToActorMessage(this Item self,string fromEmailAddress)
        {
            //return new SimpleMessage<string>(self);
            EmailMessage msg = new EmailMessage()
            {
                Subject = self.Subject,
                Body = self.Body.Text,
                IsBodyHtml = self.Body.BodyType == BodyType.HTML,
                ToEmailAddress = self.DisplayTo,
                FromEmailAddress = fromEmailAddress
            };

            // TODO to email friendly name is missing.
            return msg;

        }
    }
}
