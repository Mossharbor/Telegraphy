using System;

namespace Usenet.Actors
{
    using System.Collections.Generic;
    using Telegraphy.Net;

    class ArticleRetrievalHander : SocketBasedActor
    {
        private string SelectNewsGroup(ServerConnection conn, string newsGroup)
        {
            //Select a group and get article list
            if (newsGroup.Equals(conn.CurrentNewsGroup))
                return conn.LastNewsGroupSelectionResponse;

            Write(conn, NNTPMessages.SelectGroup(newsGroup));

            string response = Response(conn, NNTPMessages.SelectGroup(newsGroup));

            if (response.Substring(0, 3) != NNTPResponseCodes.GroupSelected)
            {
                throw new Exception(); //TODO throw new NntpException(response);
            }

            conn.CurrentNewsGroup = new NewsGroup(conn.ServerName, response);
            conn.LastNewsGroupSelectionResponse= response;

            //TODO update the newsgroup list in conn

            return response;
        }

        public override bool OnMessageRecieved<T>(T msg)
        {
            //string getHead = typeof(Messages.GetArticleHead).ToString();
            string article = null;
            string ask = msg.GetType().ToString();

            switch (ask)
            {
                case "Usenet.Messages.GetArticleHead":
                    article = GetArticleHead(msg);
                    ((IActorMessage)msg).ProcessingResult = article;
                    break;

                case "Usenet.Messages.GetArticleBody":
                    article = GetArticleBody(msg);
                    ((IActorMessage)msg).ProcessingResult = article;
                    break;

                case "Usenet.Messages.GetArticle":
                    article = GetWholeArticle(msg);
                    ((IActorMessage)msg).ProcessingResult = article;
                    break;

                case "Usenet.Messages.GetNextArticleId":
                    var newId = GetNextArticleId(msg);
                    ((IActorMessage)msg).ProcessingResult = newId;
                    break;

                case "Usenet.Messages.GetNewArticleIds":
                    var newIds = GetNewArticleIds(msg);
                    ((IActorMessage)msg).ProcessingResult = newIds;
                    break;

                default:
                    return false;
            }

            return true;
        }

        private uint GetNextArticleId<T>(T msg)
        {
            /// the server maintains a list of what you have and have not downloaded
            /// the next message return the next message in the list

            var conn = (msg as Messages.GetNextArticleId).Connection;
            string newsGroup = (msg as Messages.GetNextArticleId).NewsGroup;
            bool commandSupported = (bool)conn.SupportedCommands[NNTPMessages.Next];

            if (!commandSupported)
            {
                throw new Exception(); //TODO
            }

            Write(conn, NNTPMessages.Next);

            string response = Response(conn, NNTPMessages.Next);

            VerifyResponse(response);

            if ((response.Substring(0, 3) == NNTPResponseCodes.NoNextArticleInThisGroup)
                || (response.Substring(0, 3) == NNTPResponseCodes.NoCurrentArticleHasBeenSelected))
            {
                throw new Exception(); //TODO:
            }

            if (response.Substring(0, 3) != NNTPResponseCodes.ArticleRetrievedRequestTextSeparatley)
            {
                throw new Exception(); //TODO throw new NntpException(response);
            }

            //parse id from response
            string id = response.Split(" ".ToCharArray())[1];

            uint articleID = Convert.ToUInt32(id);

            return articleID;
        }

        private List<uint> GetNewArticleIds<T>(T msg)
        {
            var conn = (msg as Messages.GetNewArticleIds).Connection;
            DateTime newArticlesSince = (msg as Messages.GetNewArticleIds).NewArticlesSince;
            string newsGroup = (msg as Messages.GetNewArticleIds).NewsGroup;
            bool NewNewsCommandSupported = (bool)conn.SupportedCommands[NNTPMessages.NewNews];

            if (!NewNewsCommandSupported)
            {
                //Note: this wil return the highest message index
                //conn.CurrentNewsGroup.StopIndex;
                conn.CurrentNewsGroup = null;
                string response = SelectNewsGroup(conn, newsGroup);
                LogResponse(response);
                throw new NotImplementedException(); //TODO we need to parse the stop index before query and figure out new ids.
            }
            else
            {
                string response;

                Write(conn, NNTPMessages.GetNewArticles(newsGroup, newArticlesSince));

                response = Response(conn, NNTPMessages.GetNewArticles(newsGroup, newArticlesSince));

                //Handle the NEWNEWS command not being supported.
                if ((response.Contains("NEWNEWS") && (response.Substring(0, 3) == NNTPResponseCodes.PermissionDenied))
                    || (response.Substring(0, 3) == NNTPResponseCodes.CommandNotRecognized))
                {
                    conn.SupportedCommands[NNTPMessages.NewsGroups] = false;
                    throw new Exception(); //TODO
                }

                if (response.Substring(0, 3) != NNTPResponseCodes.ListOfNewNewsgroupsFollows)
                {
                    throw new Exception(); //TODO throw new NntpException(response);
                }

                bool storedAuth = conn.Authenticate;
                conn.Authenticate = false;
                List<uint> messageIds = new List<uint>();

                //TODO fix this infinite loop.
                while (true)
                {
                    response = Response(conn, NNTPMessages.GetNewArticles(newsGroup, newArticlesSince));

                    if (response == ".\r\n")
                        break;

                    if (response == ".\n")
                        break;


                    uint articleID = Convert.ToUInt32(response);
                    messageIds.Add(articleID);
                }

                conn.Authenticate = storedAuth;

                return messageIds;
            }
        }

        private string GetWholeArticle<T>(T msg)
        {
            var conn = (msg as Messages.GetArticle).Connection;
            uint articleId = (msg as Messages.GetArticle).ArticleID;
            string newsGroup = (msg as Messages.GetArticle).NewsGroup;
            bool ArticleCommandSupported = (bool)conn.SupportedCommands[NNTPMessages.Article];

            if (!ArticleCommandSupported)
            {
                //TODO: use the article head and body command here:
                throw new NotImplementedException();
            }

            string response = SelectNewsGroup(conn, newsGroup);
            LogResponse(response);

            Write(conn,NNTPMessages.GetArticle(articleId.ToString()));

            response = Response(conn, NNTPMessages.GetArticle(articleId.ToString()));

            VerifyResponse(response);

            if (response.Substring(0, 3) == NNTPResponseCodes.NoSuchArticleNumberInGroup)
            {
                throw new Exception(); //TODO
            }

            if ((response.Substring(0, 3) != NNTPResponseCodes.ArticleRetrievedHeadAndBodyToFollow)
                && (response.Substring(0, 3) != NNTPResponseCodes.ArticleRetrievedHeadToFollow))
            {
                throw new Exception(); //TODO  throw new NntpException(response);
            }

            //NOTE: we add the ServerArticleIndex so that we may add it to the newsgroup post class later when we parse the text.
            string article = String.Empty;

            bool storedAuth = conn.Authenticate;
            conn.Authenticate = false;

            while (true)
            {
                response = Response(conn, NNTPMessages.GetArticle(articleId.ToString()));
                if (response == ".\r\n")
                    break;

                if (response == ".\n")
                    break;

                //if (article.Length < 1024)
                article += response;
            }

            conn.Authenticate = storedAuth;

            return article;
        }

        private string GetArticleBody<T>(T msg)
        {
            var conn = (msg as Messages.GetArticleBody).Connection;
            uint articleId = (msg as Messages.GetArticleBody).ArticleID;
            string newsGroup = (msg as Messages.GetArticleBody).NewsGroup;
            bool BodyCommandSupported = (bool)conn.SupportedCommands[NNTPMessages.Body];

            if (!BodyCommandSupported)
                throw new Exception(); //TODO:

            string response = SelectNewsGroup(conn, newsGroup);
            LogResponse(response);

            Write(conn, NNTPMessages.GetArticleBody(articleId.ToString()));
            response = Response(conn, NNTPMessages.GetArticleBody(articleId.ToString()));

            LogResponse(response);
            if (!NNTPResponseCodes.IsResposeCode(response, NNTPResponseCodes.ArticleRetrievedBodyToFollow))
            {
                if (response.Length <= 0)
                    throw new Exception();// TODO
            }

            if (NNTPResponseCodes.IsResposeCode(response, NNTPResponseCodes.NoSuchArticleNumberInGroup))
            {
                throw new Exception(); //TODO:
            }

            string articleBody = "";

            bool storedAuth = conn.Authenticate;
            conn.Authenticate = false;

            while (true)
            {
                response = Response(conn, NNTPMessages.GetArticleBody(articleId.ToString()));
                if (response == ".\r\n")
                    break;

                if (response == ".\n")
                    break;

                //if (article.Length < 1024)
                articleBody += response;
            }

            conn.Authenticate = storedAuth;

            return articleBody;
        }

        private string GetArticleHead<T>(T msg)
        {
            bool stillNeedToRetrievearticleLines = true;
            var conn = (msg as Messages.GetArticleHead).Connection;
            uint articleId = (msg as Messages.GetArticleHead).ArticleID;
            string newsGroup = (msg as Messages.GetArticleHead).NewsGroup;
            bool HeadCommandSupported = (bool)conn.SupportedCommands[NNTPMessages.Head];

            if (!HeadCommandSupported)
                throw new Exception(); //TODO:

            string response = SelectNewsGroup(conn, newsGroup);
            LogResponse(response);

            string msgString = NNTPMessages.GetArticleHead(articleId.ToString());
            Write(conn, msgString);

            response = Response(conn, msgString);

            VerifyResponse(response);

            if (response.Substring(0, 3) == NNTPResponseCodes.NoSuchArticleNumberInGroup)
            {
                throw new Exception(); //TODO:
            }

            if ((response.Substring(0, 3) != NNTPResponseCodes.ArticleRetrievedHeadAndBodyToFollow)
                && (response.Substring(0, 3) != NNTPResponseCodes.ArticleRetrievedHeadToFollow))
            {
                throw new Exception(); // TODO throw new NntpException(response);
            }

            bool storedAuth = conn.Authenticate;
            conn.Authenticate = false;
            uint linesInArticle = 0;
            string article = String.Empty;

            while (true)
            {
                response = Response(conn, msgString);
                if (response == ".\r\n")
                    break;

                if (response == ".\n")
                    break;

                //see how many lines the article has to get body later.
                if ((HeadCommandSupported) && (stillNeedToRetrievearticleLines) && (response.Contains("Lines:")))
                {
                    string articleLines = response.Split(":".ToCharArray())[1];

                    linesInArticle = Convert.ToUInt32(articleLines.Trim());
                    stillNeedToRetrievearticleLines = false;
                }

                article += response;
            }

            LogResponse(article);
            conn.Authenticate = storedAuth;
            return article;
        }
    }
}
