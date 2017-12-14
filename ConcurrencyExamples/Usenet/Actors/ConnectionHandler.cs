using System;

namespace Usenet.Actors
{
    using Telegraphy.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class ConnectionHandler : SocketBasedActor
    {
        public ConnectionHandler()
        {
        }

        public override bool OnMessageRecieved<T>(T msg) 
        {
            if (msg.GetType() == typeof(Messages.ConnectToServer))
            {
                return Connect(msg);
            }
            else if (msg.GetType() == typeof(Messages.DisconnectFromServer))
            {
                return Disconnect(msg);
            }

            return false;
        }

        private bool Disconnect<T>(T msg)
        {
            /// The method will send a QUIT message to the server and the server should respond with a 205 status-code indicating that the it is disconnecting the socket.
            /// When you first instantiate the Nntp object you should call the Connect method and when you are finished you should call the Disconnect method.
            /// In between, you can call three method, GetNewsgroups, GetNews and Post, to receive and send news to the NNTP server. 
            /// The GetNewsgroups method, receives from the NNTP server all the forums that are supported by the server.
            
            var connection = (msg as Messages.DisconnectFromServer).Connection;

            Write(connection,NNTPMessages.Quit);

            string response = Response(connection, NNTPMessages.Quit);

            if (response.Substring(0, 3) != NNTPResponseCodes.ClosingConnectionGoodbye)
            {
                //LibrayMessageLogger.Instance.WriteError(this, NNTPErrorTypes.NNTPResponseWasNotTheOneExpected, response);
                throw new Exception(); // TODO throw new NntpException(response);
            }

            connection.Connection.Client.Shutdown(SocketShutdown.Both);
            connection.Connection.Client.Close();

            return true;
        }

        private bool Connect<T>(T msg)
        {
            /// We call the Connect method of our base TcpClient class with the server name and port 119. Port 119 is the well-known port for NNTP servers. 
            /// The server should respond with a 200 status-code indicating that connection was successful.
            /// When you are finished calling methods to the Nntp client object, then you should call the Disconnect method to terminate the connection.
            
            var connection = (msg as Messages.ConnectToServer).Connection;
            string server = connection.ServerName;
            TcpClient client = new TcpClient();
            System.Diagnostics.Debug.WriteLine("Connecting:" + server);
            client.Connect(server, 119);
            NetworkStream stream = client.GetStream();
            stream.ReadTimeout = 30 * 1000;

            connection.Connection = client;
            connection.Stream = stream;

            System.Diagnostics.Debug.WriteLine("Connected asking for response:" + server);
            Task<IActorMessage> t = Telegraph.Instance.Ask(new Messages.GetReponse(connection, ""));

            t.Wait();

            string response = (t.Result.ProcessingResult as string);

            Telegraph.Instance.Tell(response);

            if (NNTPResponseCodes.IsResposeCode(response, NNTPResponseCodes.ServerReadyNoPostingAllowed))
            {
                connection.SupportedCommands[NNTPMessages.Post] = false;
            }
            else if (response.Substring(0, 3) != NNTPResponseCodes.ServerReadyPostingAllowed)
            {
                throw new Exception(); // TODO:throw new NntpException(response);
            }
            else
                connection.SupportedCommands[NNTPMessages.Post] = true;

            return true;
        }
    }
}
