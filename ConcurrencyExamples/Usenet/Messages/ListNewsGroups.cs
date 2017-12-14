using System;

namespace Usenet.Messages
{
    class ListNewsGroups : ConnectedMessageBase
    {
        public ListNewsGroups(ServerConnection conn)
        {
            this.Connection = conn;
            this.ThisType = typeof(ListNewsGroups);
        }
    }
}
