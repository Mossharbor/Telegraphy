using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Actors
{
    class ArticlePostHandler: SocketBasedActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            if (msg.GetType() != typeof(Messages.PostArticle))
                return false;

            throw new NotImplementedException();
        }
    }
}
