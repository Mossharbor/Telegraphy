using Microsoft.Azure.Relay;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Net.TPLExtentions;

namespace Telegraphy.Azure.Relay.Hybrid
{
    public class HybridConnectionBaseOperator<MsgType> : IOperator where MsgType : class
    {
        internal const int DefaultConcurrency = 1;
        private HybridConnectionListener listener = null;
        private Semaphore _dataExists = new Semaphore(0, int.MaxValue);
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        IProducerConsumerCollection<IActorMessage> actorMessages = new ConcurrentQueue<IActorMessage>();
        string hybridConnectionString;
        string hybridConnectionName;
        RelayConnectionStringBuilder connectionItems = null;

        internal HybridConnectionBaseOperator(ILocalSwitchboard switchboard, string hybridConnectionString, bool createConnectionIfItDoesNotExist)
        {
            connectionItems = new RelayConnectionStringBuilder(hybridConnectionString);
            if (String.IsNullOrEmpty(connectionItems.EntityPath))
                throw new HybridConnectionNameNoSpecifiedInEntityPathException(hybridConnectionString);

            this.hybridConnectionString = hybridConnectionString;
            this.hybridConnectionName = connectionItems.EntityPath;

            if (createConnectionIfItDoesNotExist)
                CreateHybridConnectionIfDoesNotExist();

            if (null != switchboard)
            {
                this.Switchboards.Add(switchboard);
                switchboard.Operator = this;
            }

            StartListener();
        }

        internal HybridConnectionBaseOperator(ILocalSwitchboard switchboard, string hybridConnectionString, string hybridConnectionName, bool createConnectionIfItDoesNotExist)
        {
            connectionItems = new RelayConnectionStringBuilder(hybridConnectionString) { EntityPath = hybridConnectionName };
            if (String.IsNullOrEmpty(connectionItems.EntityPath))
                throw new HybridConnectionNameNoSpecifiedInEntityPathException(hybridConnectionString);

            this.hybridConnectionString = hybridConnectionString;
            this.hybridConnectionName = hybridConnectionName;

            if (createConnectionIfItDoesNotExist)
                CreateHybridConnectionIfDoesNotExist();

            if (null != switchboard)
            {
                this.Switchboards.Add(switchboard);
                switchboard.Operator = this;
            }

            StartListener();
        }

        private void CreateHybridConnectionIfDoesNotExist()
        {
            var t = new RelayConnectionStringBuilder(hybridConnectionString);
            t.EntityPath = string.Empty;
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(t.ToString().TrimEnd(';'));
            HybridConnectionDescription rd;
            if (!ns.HybridConnectionExists(hybridConnectionName, out rd))
                ns.CreateHybridConnection(hybridConnectionName);
        }

        private HybridConnectionListener CreateHybridListener()
        {
            //var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            //var uri = new Uri(string.Format("https://{0}/{1}", connectionStringBuilder.Endpoint.Host, hybridConnectionName));
            listener = new HybridConnectionListener(connectionItems.ToString());

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { System.Diagnostics.Debug.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { System.Diagnostics.Debug.WriteLine("Offline"); };
            listener.Online += (o, e) => { System.Diagnostics.Debug.WriteLine("Online"); };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                //TODO if string then handle response
                IActorMessage msg = null;
                if (typeof(MsgType) == typeof(string))
                {
                    var reader = new StreamReader(context.Request.InputStream);
                    string msgString = reader.ReadToEnd();
                    msg = msgString.ToActorMessage();
                }
                else if (typeof(MsgType) == typeof(byte[]))
                {
                    var reader = new BinaryReader(context.Request.InputStream);
                    byte[] msgBytes = reader.ReadBytes(1024);
                    msg = msgBytes.ToActorMessage();
                }
                else
                {
                    byte[] msgBytes = new byte[context.Request.InputStream.Length];
                    var reader = new BinaryReader(context.Request.InputStream);
                    reader.Read(msgBytes, 0, msgBytes.Length);
                    msg = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(msgBytes)).Result;
                }

                if (null == msg.Status)
                    msg.Status = new TaskCompletionSource<IActorMessage>();

                // Store the message in the global queue, 
                // call Telegraphy.Net.TPLExtentions Task.Then to send the result back to the relay
                // after we are done processing it
                msg.Status.Task.Then(() =>
                {
                    // Do something with context.Request.Url, HttpMethod, Headers, InputStream...
                    context.Response.StatusCode = HttpStatusCode.OK;
                    context.Response.StatusDescription = "OK";
                    // The context MUST be closed here
                    context.Response.Close();

                    //using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        var relay = new RecieveResponseFromRequest<MsgType, MsgType>(connectionItems.ToString());
                        relay.Tell(msg.Status.Task.Result); // send the answer back to the calling application.
                        //sw.Write(responseMessage);
                    }
                });

                NextMessage = msg;
            };

            listener.OpenAsync().Wait();
            System.Diagnostics.Debug.WriteLine("Hybrid Connection Operator Listening");
            return listener;
        }

        private IActorMessage NextMessage
        {
            get
            {
                bool capturedLock = _dataExists.WaitOne(new TimeSpan(0, 0, 1)); // wait until there is something to process.

                IActorMessage next = null;
                if (!actorMessages.TryTake(out next))
                {
                    return null;
                }
                return next;
            }
            set
            {
                actorMessages.TryAdd(value);
                try { _dataExists.Release(); }
                catch (SemaphoreFullException) { }
            }
        }

        private void StartListener()
        {
            this.listener = CreateHybridListener();
        }

        private void StopListener()
        {
            if (null != listener)
                listener.CloseAsync().Wait();
        }

        #region IOperator
        public long ID { get; set; }

        public ulong Count => throw new NotImplementedException();

        private List<ILocalSwitchboard> _switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards
        {
            get { return _switchboards; }
        }

        public void AddMessage(IActorMessage msg)
        {
            this.NextMessage = msg;
        }

        public IActorMessage GetMessage()
        {
            return NextMessage;
        }

        public void Kill()
        {
            try { _dataExists.Release(); }
            catch (SemaphoreFullException) { }

            StopListener();

            foreach (var switchBoard in this.Switchboards)
                switchBoard.Disable();
        }

        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            StopListener();
            DateTime start = DateTime.Now;
            while (actorMessages.Any())
            {
                System.Threading.Thread.Sleep(1000);

                if ((DateTime.Now - start) > timeout)
                    return false;
            }

            return true;
        }
        #endregion

        #region IActor
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            AddMessage(msg);
            return true;
        }
        #endregion  
    }
}
