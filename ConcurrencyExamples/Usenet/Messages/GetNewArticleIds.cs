using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    class GetNewArticleIds :ConnectedMessageBase
    {
        string newsGroup;
        public string NewsGroup
        {
            get { return newsGroup; }
            set { newsGroup = value; }
        }

        private DateTime newArticlesSince;
        public DateTime NewArticlesSince
        {
            get { return newArticlesSince; }
            set { newArticlesSince = value; }
        }

        public GetNewArticleIds(ServerConnection conn, string newsGroup, DateTime newArticlesSince)
        {
            this.Connection = conn;
            this.NewArticlesSince = newArticlesSince;
            this.NewsGroup = newsGroup;
            this.ThisType = typeof(GetNewArticleIds);
        }
    }
}
