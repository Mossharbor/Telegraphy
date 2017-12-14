using System;

namespace Usenet.Messages
{
    public class ConnectToServer : ConnectedMessageBase
    {
        public ConnectToServer(ServerConnection conn)
        {
            this.Connection = conn;
            this.ThisType = typeof(ConnectToServer);
        }
    }
}
