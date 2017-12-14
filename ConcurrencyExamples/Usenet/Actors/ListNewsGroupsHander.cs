using System;

namespace Usenet.Actors
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using Telegraphy.Net;

    class ListNewsGroupsHander: SocketBasedActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            /// The GetNewsgroups method begins by sending a LIST message to the NNTP server. 
            /// The NNTP server will respond initially with the 215 status-code indicating that it successfully received the LIST message. 
            /// Then the NNTP server will respond with a series of lines, each representing one forum on the NNTP server. 
            /// After all the forums are sent, the NNTP server will send one line with a single period, indicating the end of the forum list.
            /// The list of forums is returned from the GetNewsgroups method as an ArrayList of strings.
            /// From the list of forums, you can select one forum and receive from the GetNews method all the news for that forum. 
            /// Call GetNews passing the name of the forum to receive the news postings.
            
            if (msg.GetType() != typeof(Messages.ListNewsGroups)
                && msg.GetType() != typeof(Messages.ListNewsGroupsSince))
                return false;

            var conn = (msg as Messages.ListNewsGroups).Connection;

            if (msg.GetType() != typeof(Messages.ListNewsGroupsSince))
                Write(conn,NNTPMessages.ListNewsGroups);
            else
                Write(conn, NNTPMessages.ListNewsGroupsSince((msg as Messages.ListNewsGroupsSince).LastCheckedDate));

            var response = Response(conn, NNTPMessages.ListNewsGroups);
            LogResponse(response);

            if (response.Substring(0, 3) != NNTPResponseCodes.ListOfNewsGroupsFollows)
                throw new Exception(); //TODO throw new NntpException(response);

            var list = GetNewsGroupListFromResponse(conn, NNTPMessages.ListNewsGroups);

            return true;
        }

        /// <summary>
        /// Parses the incoming list of newsgroups from the response stream.
        /// </summary>
        /// <param name="conn">This is the server connection to use</param>
        /// <returns>An array list of strings containing the newsgroups.</returns>
        private IEnumerable<NewsGroup> GetNewsGroupListFromResponse(ServerConnection conn,string lastMessage)
        {
            string response = String.Empty;
            List<NewsGroup> retval = new List<NewsGroup>();

            bool storedAuthentication = conn.Authenticate;
            conn.Authenticate = false;

            while (response != ".\r\n" && response != ".\n")
            {
                response = Response(conn, lastMessage);
                LogResponse(response);

                NewsGroup newsGroup = new NewsGroup(conn.ServerName, response);
                retval.Add(newsGroup);
            }

            conn.Authenticate = storedAuthentication;

            return retval;
        }
    }
}
