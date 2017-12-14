using System;

namespace Usenet.Actors
{
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using Telegraphy.Net;

    class PopulateCommandList : SocketBasedActor
    {
        private void AddValidCommand(ServerConnection conn, string command)
        {
            //TODO: track the unknown COmmands
            //List<string> unknownCOmmands;
            command = command.ToUpper().Trim();

            if (command.StartsWith("AUTHINFO"))
            {
                bool foundAuthInfo = false;

                if (command.Contains("USER NAME|PASS PASSWORD"))
                {
                    conn.SupportedCommands[NNTPMessages.AuthInfoUser] = true;
                    conn.SupportedCommands[NNTPMessages.AuthInfoPass] = true;
                    foundAuthInfo = true;
                }

                if (command.Contains("|GENERIC"))
                {
                    conn.SupportedCommands[NNTPMessages.AuthInfoGeneric] = true;
                    foundAuthInfo = true;
                }

                //if (!foundAuthInfo)
                //    unknownCOmmands.Add(command);

                return;
            }

            if (command.StartsWith(NNTPMessages.Article))
            {
                conn.SupportedCommands[NNTPMessages.Article] = true;
                return;
            }

            if (command.StartsWith(NNTPMessages.ListNewsGroups))
            {
                conn.SupportedCommands[NNTPMessages.ListNewsGroups] = true;
                return;
            }

            if (command.StartsWith(NNTPMessages.NewsGroups))
            {
                conn.SupportedCommands[NNTPMessages.NewsGroups] = true;
                return;
            }

            if (command.StartsWith(NNTPMessages.Head))
            {
                conn.SupportedCommands[NNTPMessages.Head] = true;
                return;
            }

            if (command.StartsWith(NNTPMessages.Body))
            {
                conn.SupportedCommands[NNTPMessages.Body] = true;
                return;
            }

            switch (command)
            {
                case NNTPMessages.Ihave:
                    conn.SupportedCommands[NNTPMessages.Ihave] = true;
                    break;

                case NNTPMessages.Help:
                    conn.SupportedCommands[NNTPMessages.Help] = true;
                    break;

                case NNTPMessages.Next:
                    conn.SupportedCommands[NNTPMessages.Next] = true;
                    break;

                case NNTPMessages.Post:
                    conn.SupportedCommands[NNTPMessages.Post] = true;
                    break;

                case NNTPMessages.Slave:
                    conn.SupportedCommands[NNTPMessages.Slave] = true;
                    break;

                case NNTPMessages.Group:
                    conn.SupportedCommands[NNTPMessages.Group] = true;
                    break;
            }

        }

        public override bool OnMessageRecieved<T>(T msg)
        {
            if (msg.GetType() != typeof(Messages.PopulateCommandList))
                return false;

            var conn = (msg as Messages.PopulateCommandList).Connection;

            conn.ResetSupportedCommandList();
            Write(conn, NNTPMessages.Help);

            string response = Response(conn, NNTPMessages.Help);

            Telegraph.Instance.Tell(response);

            if (!NNTPResponseCodes.IsResposeCode(response, NNTPResponseCodes.HelpTextFollows))
            {
                //LibrayMessageLogger.Instance.WriteError(this, NNTPErrorTypes.NNTPResponseWasNotTheOneExpected, response);
                throw new Exception(); // throw new NntpException(response);
            }

            bool storeadAuth = conn.Authenticate;
            conn.Authenticate = false;

            while (true)
            {
                response = Response(conn,NNTPMessages.Help);
                if (response == ".\r\n")
                    break;

                if (response == ".\n")
                    break;

                Telegraph.Instance.Tell(response);
                AddValidCommand(conn,response);
            }

            conn.Authenticate = storeadAuth;
            return true;
        }

    }
}
