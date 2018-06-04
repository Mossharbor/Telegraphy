using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Usenet
{
    using System.Xml.Linq;
    using Telegraphy.Net;
    using System.Threading.Tasks;
    using System.Net.Sockets;

    class P
    {
        private class UseNetClient
        {
            public static object consoleLock = new object();

            public static void RegisterActorsAndMessages(uint numberOfSimultaneousConnections)
            {
                IOperator connectionOperator = new LocalQueueOperator(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount,4));
                IOperator responseOperator = new LocalQueueOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool));
                IOperator perConnection = new LocalQueueOperator(new LocalSwitchboard(LocalConcurrencyType.DedicatedThreadCount, numberOfSimultaneousConnections));
                try
                {
                    Telegraph.Instance.MessageDispatchProcedure = MessageDispatchProcedureType.RoundRobin;
                    Telegraph.Instance.Register(connectionOperator);
                    Telegraph.Instance.Register(responseOperator);
                    Telegraph.Instance.Register(perConnection);
                    long opID = perConnection.ID;
                    long connID = connectionOperator.ID;
                    long respID = responseOperator.ID;

                    //Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool));
                    Telegraph.Instance.Register<Messages.ConnectToServer    , Actors.ConnectionHandler>(connID, ()   => new Actors.ConnectionHandler());
                    Telegraph.Instance.Register<Messages.GetReponse         , Actors.ResponseHandler>(respID,()      => new Actors.ResponseHandler());
                    Telegraph.Instance.Register<Messages.DisconnectFromServer,Actors.ConnectionHandler>(connID, ()   => new Actors.ConnectionHandler());
                    Telegraph.Instance.Register<Messages.Authenticate       , Actors.AuthenticateUser>(opID,()       => new Actors.AuthenticateUser());
                    Telegraph.Instance.Register<Messages.PopulateCommandList, Actors.PopulateCommandList>(opID,()    => new Actors.PopulateCommandList());
                    Telegraph.Instance.Register<Messages.RequestSlaveStatus , Actors.SlaveStatusHandler>(opID,()     => new Actors.SlaveStatusHandler());
                    Telegraph.Instance.Register<Messages.ListNewsGroups     , Actors.ListNewsGroupsHander>(opID,()   => new Actors.ListNewsGroupsHander());
                    Telegraph.Instance.Register<Messages.ListNewsGroupsSince, Actors.ListNewsGroupsHander>(opID,()   => new Actors.ListNewsGroupsHander());
                    Telegraph.Instance.Register<Messages.GetArticleHead     , Actors.ArticleRetrievalHander>(opID,() => new Actors.ArticleRetrievalHander());
                    Telegraph.Instance.Register<Messages.GetArticleBody     , Actors.ArticleRetrievalHander>(opID,() => new Actors.ArticleRetrievalHander());
                    Telegraph.Instance.Register<Messages.GetArticle         , Actors.ArticleRetrievalHander>(opID,() => new Actors.ArticleRetrievalHander());
                    Telegraph.Instance.Register<Messages.PostArticle        , Actors.ArticlePostHandler>(opID,()     => new Actors.ArticlePostHandler());

                    Telegraph.Instance.Register<string>(connectionOperator.ID, 
                        message => 
                        {
                            lock (consoleLock)
                            {
                                Console.WriteLine(message.Replace(System.Environment.NewLine, ""));
                            }
                        });
                }
                catch (FailedRegistrationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(ex.GetType().ToString() + ":" + ex.Message);
                    Console.ResetColor();
                    throw;
                }

                RegisterErrorHandling(perConnection.ID);
                RegisterErrorHandling(connectionOperator.ID);
            }

            public static void RegisterErrorHandling(long operatorID)
            {
                #region Handle Socket Exceptions
                Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> socketExceptionHandler = delegate(Exception ex, IActor actor, IActorMessage msg, IActorInvocation invoker)
                {
                    switch ((SocketError)(ex as SocketException).ErrorCode)
                    {
                        #region unhandled socket errors
                        case SocketError.SocketError: // -1,
                        //     The System.Net.Sockets.Socket operation succeeded.
                        //case SocketError.Success: // 0,
                        //     The overlapped operation was aborted due to the closure of the System.Net.Sockets.Socket.
                        case SocketError.OperationAborted: // 995,
                        //     The application has initiated an overlapped operation that cannot be completed
                        //     immediately.
                        case SocketError.IOPending: // 997,
                        //     A blocking System.Net.Sockets.Socket call was canceled.
                        case SocketError.Interrupted: // 10004,
                        //     An attempt was made to access a System.Net.Sockets.Socket in a way that is
                        //     forbidden by its access permissions.
                        case SocketError.AccessDenied: // 10013,
                        //     An invalid pointer address was detected by the underlying socket provider.
                        case SocketError.Fault: // 10014,
                        //     An invalid argument was supplied to a System.Net.Sockets.Socket member.
                        case SocketError.InvalidArgument: // 10022,
                        //     There are too many open sockets in the underlying socket provider.
                        case SocketError.TooManyOpenSockets: // 10024,
                        //     An operation on a nonblocking socket cannot be completed immediately.
                        case SocketError.WouldBlock: // 10035,
                        //     A blocking operation is in progress.
                        case SocketError.InProgress: // 10036,
                        //     The nonblocking System.Net.Sockets.Socket already has an operation in progress.
                        case SocketError.AlreadyInProgress: // 10037,
                        //     A System.Net.Sockets.Socket operation was attempted on a non-socket.
                        case SocketError.NotSocket: // 10038,
                        //     A required address was omitted from an operation on a System.Net.Sockets.Socket.
                        case SocketError.DestinationAddressRequired: // 10039,
                        //     The datagram is too long.
                        case SocketError.MessageSize: // 10040,
                        //     The protocol type is incorrect for this System.Net.Sockets.Socket.
                        case SocketError.ProtocolType: // 10041,
                        //     An unknown, invalid, or unsupported option or level was used with a System.Net.Sockets.Socket.
                        case SocketError.ProtocolOption: // 10042,
                        //     The protocol is not implemented or has not been configured.
                        case SocketError.ProtocolNotSupported: // 10043,
                        //     The support for the specified socket type does not exist in this address
                        //     family.
                        case SocketError.SocketNotSupported: // 10044,
                        //     The address family is not supported by the protocol family.
                        case SocketError.OperationNotSupported: // 10045,
                        //     The protocol family is not implemented or has not been configured.
                        case SocketError.ProtocolFamilyNotSupported: // 10046,
                        //     The address family specified is not supported. This error is returned if
                        //     the IPv6 address family was specified and the IPv6 stack is not installed
                        //     on the local machine. This error is returned if the IPv4 address family was
                        //     specified and the IPv4 stack is not installed on the local machine.
                        case SocketError.AddressFamilyNotSupported: // 10047,
                        //     Only one use of an address is normally permitted.
                        case SocketError.AddressAlreadyInUse: // 10048,
                        //     The selected IP address is not valid in this context.
                        case SocketError.AddressNotAvailable: // 10049,
                        //     The network is not available.
                        case SocketError.NetworkDown: // 10050,
                        //     No route to the remote host exists.
                        case SocketError.NetworkUnreachable: // 10051,
                        //     The application tried to set System.Net.Sockets.SocketOptionName.KeepAlive
                        //     on a connection that has already timed out.
                        case SocketError.NetworkReset: // 10052,
                        //     The connection was aborted by the .NET Framework or the underlying socket
                        //     provider.
                        case SocketError.ConnectionAborted: // 10053,
                        //     The connection was reset by the remote peer.
                        //case SocketError.ConnectionReset: // 10054,
                        //     No free buffer space is available for a System.Net.Sockets.Socket operation.
                        case SocketError.NoBufferSpaceAvailable: // 10055,
                        //     The System.Net.Sockets.Socket is already connected.
                        case SocketError.IsConnected: // 10056,
                        //     The application tried to send or receive data, and the System.Net.Sockets.Socket
                        //     is not connected.
                        case SocketError.NotConnected: // 10057,
                        //     A request to send or receive data was disallowed because the System.Net.Sockets.Socket
                        //     has already been closed.
                        case SocketError.Shutdown: // 10058,
                        //     The connection attempt timed out, or the connected host has failed to respond.
                        case SocketError.TimedOut: // 10060,
                        //     The remote host is actively refusing a connection.
                        case SocketError.ConnectionRefused: // 10061,
                        //     The operation failed because the remote host is down.
                        case SocketError.HostDown: // 10064,
                        //     There is no network route to the specified host.
                        case SocketError.HostUnreachable: // 10065,
                        //     Too many processes are using the underlying socket provider.
                        case SocketError.ProcessLimit: // 10067,
                        //     The network subsystem is unavailable.
                        case SocketError.SystemNotReady: // 10091,
                        //     The version of the underlying socket provider is out of range.
                        case SocketError.VersionNotSupported: // 10092,
                        //     The underlying socket provider has not been initialized.
                        case SocketError.NotInitialized: // 10093,\
                        //     A graceful shutdown is in progress.
                        case SocketError.Disconnecting: // 10101,\
                        //     The specified class was not found.
                        case SocketError.TypeNotFound: // 10109,\
                        //     No such host is known. The name is not an official host name or alias.
                        //case SocketError.HostNotFound: // 11001,\
                        //     The name of the host could not be resolved. Try again later.
                        //case SocketError.TryAgain: // 11002,\
                        //     The error is unrecoverable or the requested database cannot be located.
                        case SocketError.NoRecovery: // 11003,
                        //     The requested name or IP address was not found on the name server.
                        case SocketError.NoData: // 11004,\e closure of the System.Net.Sockets.Socket.
                            throw new NotImplementedException();
                        #endregion

                        case SocketError.HostNotFound:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine(ex.Message);
                            Console.ResetColor();
                            msg.Status.TrySetException(ex);
                            break;

                        case SocketError.ConnectionReset:
                            break;

                        case SocketError.TryAgain:
                            // send the msg back to the queue
                            //Console.WriteLine("Sending msg back to the queue.");
                            Telegraph.Instance.Tell(msg);
                            break;
                    }

                    return null;
                };
                #endregion

                Telegraph.Instance.Register(operatorID, typeof(SocketException), socketExceptionHandler);
            }

            public static void RunClientTest(ServerConnection conn)
            {
                ServerConnection[] conns = new ServerConnection[(int)conn.MaxConcurrentConnectionCount];
                List<Task<IActorMessage>> connectionTasks = new List<Task<IActorMessage>>();

                for (int i = 0; i < conn.MaxConcurrentConnectionCount; ++i)
                {
                    conns[i] = new ServerConnection(conn.ServerName, conn.UserName, conn.Password, conn.MaxConcurrentConnectionCount, conn.Authenticate);
                    var connctionRequest = new Messages.ConnectToServer(conns[i]);
                    connectionTasks.Add(Telegraph.Instance.Ask(connctionRequest));
                }

                Task.WaitAll(connectionTasks.ToArray());
                conn = conns[0];

                var getCommands = new Messages.PopulateCommandList(conn);
                var populateCommandAsk = Telegraph.Instance.Ask(getCommands);
                populateCommandAsk.Wait();

                Console.WriteLine("Connected!!");
                for (int i = 1; i < conn.MaxConcurrentConnectionCount; ++i)
                {
                    conns[i].SupportedCommands = conns[0].SupportedCommands;
                }

                var listNewsGroup = new Messages.ListNewsGroups(conn);
                var listNewsGroupAsk = Telegraph.Instance.Ask(listNewsGroup);
                listNewsGroupAsk.Wait();

                //Retrieve Articles on multiple threads.
                uint articleStartID = 25;
                List<Task<IActorMessage>> articleRetrievalTasks = new List<Task<IActorMessage>>();
                for (uint i = 0; i < conn.MaxConcurrentConnectionCount; ++i)
                {
                    var getArticleHead = new Messages.GetArticleHead(conns[i], "microsoft.public.windows.vista.mail", articleStartID+i);
                    articleRetrievalTasks.Add(Telegraph.Instance.Ask(getArticleHead));
                }
                Task.WaitAll(articleRetrievalTasks.ToArray());

                foreach (var t in articleRetrievalTasks)
                {
                    string val = (t.Result.ProcessingResult as string);
                    Telegraph.Instance.Tell(val);
                }

                var getArticleBody = new Messages.GetArticleBody(conn, "microsoft.public.windows.vista.mail", 25);
                var getArticleBodyAsk = Telegraph.Instance.Ask(getArticleBody);
                getArticleBodyAsk.Wait();

                var getNextArticle = new Messages.GetNextArticleId(conn, "microsoft.public.windows.vista.mail");
                var getNextArticleAsk = Telegraph.Instance.Ask(getNextArticle);
                getNextArticleAsk.Wait();
                uint nextID = (uint)getNextArticleAsk.Result.ProcessingResult;

                var getNextArticleHead = new Messages.GetArticleHead(conn, "microsoft.public.windows.vista.mail", nextID);
                var getNextArticleHeadAsk = Telegraph.Instance.Ask(getNextArticleHead);
                getNextArticleHeadAsk.Wait();

                var postArticle = new Messages.PostArticle(conn,"microsoft.public.nntp.test");
                postArticle.Message = "Testing";
                var postArticleAsk = Telegraph.Instance.Ask(postArticle);
                postArticleAsk.Wait();
                
                List<Task<IActorMessage>> disconnectTasks = new List<Task<IActorMessage>>();
                for (uint i = 0; i < conn.MaxConcurrentConnectionCount; ++i)
                {
                    var disconnect = new Messages.DisconnectFromServer(conns[i]);
                    disconnectTasks.Add(Telegraph.Instance.Ask(disconnect));
                }

                Task.WaitAll(disconnectTasks.ToArray());
            }
        }

        private class UseNetScannerClient
        {
            public static void RunTestClient(string pathToServerXmlList)
            {
                List<Task<IActorMessage>> connectionTasks = new List<Task<IActorMessage>>();
                XElement el = XElement.Load(pathToServerXmlList);

                foreach (var hostinfo in el.Elements("hostinfo"))
                {
                    string hostName = hostinfo.Element("hostname").Value;

                    ServerConnection conn = new ServerConnection(hostName, "", "", 1, false);

                    var connctionRequest = new Messages.ConnectToServer(conn);
                    connectionTasks.Add(Telegraph.Instance.Ask(connctionRequest));
                    System.Threading.Thread.Sleep(100);

                    //var disconnectRequest = new Messages.DisconnectFromServer(conn);
                    //connectionTasks.Add(Telegraph.Instance.Ask(disconnectRequest));
                }

                Task.WaitAll(connectionTasks.ToArray());

                Telegraph.Instance.WaitTillEmpty(new TimeSpan(0,10,0));
            }
        }
        static void Main(string[] args)
        {
            string pathToServerXmlList = @"C:\src\Libs\Telegraph.net\Usenet\Servers.xml";
            string serverName = "freenews.netfront.net";
            //string serverName = "news.club.cc.cmu.edu";
            //reader80.eternal-september.org port 80
            //string serverName = "news.eternal-september.org"; //NOTE: Port : 563 (encrypted connection NNTPS)
            //string serverName = "freetext.usenetserver.com";
            string userName = "";
            string password = "";

            ServerConnection conn = new ServerConnection(serverName, userName, password, 2, false);

            UseNetClient.RegisterActorsAndMessages(conn.MaxConcurrentConnectionCount);

            UseNetScannerClient.RunTestClient(pathToServerXmlList);
            UseNetClient.RunClientTest(conn);

            System.Threading.Thread.Sleep(1000);
        }
    }
}
