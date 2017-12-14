using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    using Telegraphy.Net;

    public abstract class ConnectedMessageBase : SimpleMessage<ConnectedMessageBase>
    {
        public ServerConnection Connection { get; set; }
    }
}
