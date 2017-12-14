using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    class PostArticle: ConnectedMessageBase
    {
        public PostArticle(ServerConnection conn,string newsGroup)
        {
            this.Connection = conn;
            this.ThisType = typeof(Authenticate);
        }

        public string Atricle
        {
            get;
            set;
        }

        public string NewsGroup
        {
            get;
            set;
        }

    }
}
