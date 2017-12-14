using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Actors
{
    class SelectNewsGroupHandler : SocketBasedActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            //Tells the server which group we are about to get article list from
            throw new NotImplementedException();
        }

    }
}
