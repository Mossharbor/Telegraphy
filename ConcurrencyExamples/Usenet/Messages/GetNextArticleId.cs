using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    class GetNextArticleId : ConnectedMessageBase
    { 
        string newsGroup;
        public string NewsGroup
        {
            get { return newsGroup; }
            set { newsGroup = value; }
        }


        public GetNextArticleId(ServerConnection conn, string newsGroup)
        {
            this.Connection = conn;
            this.NewsGroup = newsGroup;
            this.ThisType = typeof(GetNextArticleId);
        }
    }
}
