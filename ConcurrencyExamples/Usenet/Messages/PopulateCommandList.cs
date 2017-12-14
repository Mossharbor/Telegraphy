using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    using Telegraphy.Net;

    class PopulateCommandList : SimpleMessage<PopulateCommandList>
    {
        public PopulateCommandList(ServerConnection conn)
        {
            this.Connection = conn;
            this.ThisType = typeof(PopulateCommandList);
        }

        public ServerConnection Connection { get; set; }
    }
}
