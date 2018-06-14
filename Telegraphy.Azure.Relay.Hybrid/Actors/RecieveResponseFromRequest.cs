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
    public class RecieveResponseFromRequest<MsgType> : RecieveResponseFromRequestByType, IActor where MsgType : class
    {
        public RecieveResponseFromRequest(string relayConnectionString)
            : base(typeof(MsgType), relayConnectionString)
        {
        }

        public RecieveResponseFromRequest(string relayConnectionString, string hybridConnectionName)
            : base (typeof(MsgType), relayConnectionString, hybridConnectionName)
        {
        }
    }
}
