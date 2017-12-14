using System;

namespace Usenet.Messages
{
    class Authenticate : ConnectedMessageBase
    {
        public Authenticate(ServerConnection conn)
        {
            this.Connection = conn;
            this.ThisType = typeof(Authenticate);
        }

    }
}
