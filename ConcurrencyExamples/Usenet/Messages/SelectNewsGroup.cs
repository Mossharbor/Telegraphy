using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    class SelectNewsGroup: ConnectedMessageBase
    {
        public SelectNewsGroup(ServerConnection conn)
        {
            this.Connection = conn;
            this.ThisType = typeof(SelectNewsGroup);
        }
    }
}
