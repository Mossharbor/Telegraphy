using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    using System.Net.Sockets;
    using Telegraphy.Net;

    class GetReponse : ConnectedMessageBase
    {
        public GetReponse(ServerConnection ServerInfo, string LastSentMessage)
        {
            this.Connection = ServerInfo;
            this.LastSentMessage = LastSentMessage;
            this.ThisType = typeof(GetReponse);
        }

        public string LastSentMessage { get; set; }
    }
}
