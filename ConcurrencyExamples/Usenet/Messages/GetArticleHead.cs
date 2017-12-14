using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    class GetArticleHead : ConnectedMessageBase
    {
        string newsGroup;
        public string NewsGroup
        {
            get { return newsGroup; }
            set { newsGroup = value; }
        }

        uint articleID;
        public uint ArticleID
        {
            get { return articleID; }
            set { articleID = value; }
        }

        public GetArticleHead(ServerConnection conn, string newsGroup, uint articleId)
        {
            this.Connection = conn;
            this.ArticleID = articleId;
            this.NewsGroup = newsGroup;
            this.ThisType = typeof(GetArticleHead);
        }
    }
}
