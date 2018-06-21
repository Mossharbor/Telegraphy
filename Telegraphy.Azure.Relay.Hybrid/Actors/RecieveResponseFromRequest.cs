using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.Azure.Relay;
using System.Net.Http;

namespace Telegraphy.Azure.Relay.Hybrid
{
    public class RecieveResponseFromRequest<RequestType,ResponseType> : RecieveResponseFromRequestByType, IActor where RequestType : class where ResponseType : class
    {
        public RecieveResponseFromRequest(string relayConnectionString)
            : base(typeof(RequestType), typeof(ResponseType), relayConnectionString)
        {
        }

        public RecieveResponseFromRequest(string relayConnectionString, string hybridConnectionName)
            : base (typeof(RequestType), typeof(ResponseType), relayConnectionString, hybridConnectionName)
        {
        }
    }
}
