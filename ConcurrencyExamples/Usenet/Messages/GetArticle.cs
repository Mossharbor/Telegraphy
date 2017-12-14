using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Messages
{
    class GetArticle : GetArticleHead
    {
        public GetArticle(ServerConnection conn, string newsGroup, uint articleId)
            : base(conn, newsGroup, articleId)
        {
            this.ThisType = typeof(GetArticle);
        }
    }
}
