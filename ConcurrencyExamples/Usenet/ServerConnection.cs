using System;

namespace Usenet
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Sockets;

    public class ServerConnection
    {
        public ServerConnection(string serverName, string userName, string password,uint maxConcurrentConnectionCount, bool authenticate)
        {
            this.ServerName = serverName;
            this.UserName = userName;
            this.Password = password;
            this.Authenticate = authenticate;
            this.MaxConcurrentConnectionCount = maxConcurrentConnectionCount;

            SupportedCommands = new Hashtable();
            CurrentNewsGroup = null;
            LastNewsGroupSelectionResponse = String.Empty;
        }

        public NetworkStream Stream { get; set; }
        public TcpClient Connection { get; set; }

        uint maxConcurrentConnectionCount = 1;
        public uint MaxConcurrentConnectionCount
        {
            get { return maxConcurrentConnectionCount; }
            set { maxConcurrentConnectionCount = value; }
        }

        public bool Authenticate { get; set; }

        public string ServerName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public NewsGroup CurrentNewsGroup { get; set; }
        public string LastNewsGroupSelectionResponse { get; set; }

        public Hashtable SupportedCommands { get; set; }

        public IEnumerable<NewsGroup> NewsGroups { get; set; }

        internal void ResetSupportedCommandList()
        {
            SupportedCommands.Clear();
            SupportedCommands.Add(NNTPMessages.Article, false);
            SupportedCommands.Add(NNTPMessages.Group, false);
            SupportedCommands.Add(NNTPMessages.Help, true);
            SupportedCommands.Add(NNTPMessages.NewNews, false);
            SupportedCommands.Add(NNTPMessages.ListGroup, false);
            SupportedCommands.Add(NNTPMessages.ListNewsGroups, false);
            SupportedCommands.Add(NNTPMessages.NewsGroups, false);
            SupportedCommands.Add(NNTPMessages.Next, false);
            SupportedCommands.Add(NNTPMessages.Post, false);
            SupportedCommands.Add(NNTPMessages.Quit, false);
            SupportedCommands.Add(NNTPMessages.Slave, false);
            SupportedCommands.Add(NNTPMessages.AuthInfoUser, false);
            SupportedCommands.Add(NNTPMessages.AuthInfoPass, false);
            SupportedCommands.Add(NNTPMessages.AuthInfoGeneric, false);
            SupportedCommands.Add(NNTPMessages.Head, false);
            SupportedCommands.Add(NNTPMessages.Body, false);
            SupportedCommands.Add(NNTPMessages.Ihave, false);
        }
    }
}
