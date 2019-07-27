using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Office365
{
    public class OutlookEmailPublisherOperator : OutlookEmailBaseOperator
    {
        public OutlookEmailPublisherOperator(string emailAddress, string password)
            : base(emailAddress, password)
        {

        }
    }
}
