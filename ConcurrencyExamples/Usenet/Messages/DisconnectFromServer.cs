using System;

namespace Usenet.Messages
{
    class DisconnectFromServer: ConnectedMessageBase
    {
        public DisconnectFromServer(ServerConnection conn)
        {
            this.Connection = conn;
            this.ThisType = typeof(DisconnectFromServer);
        }
    }
}
