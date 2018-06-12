using Microsoft.Azure.Relay;
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
    public class HybridConnectionOperator<MsgType> : IOperator where MsgType:class
    {
        private HybridConnectionListener listener = null;
        private Semaphore _dataExists = new Semaphore(0, int.MaxValue);
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        IProducerConsumerCollection<IActorMessage> actorMessages = null;
        string hybridConnectionString;
        string hybridConnectionName;


        public HybridConnectionOperator(string hybridConnectionString, string hybridConnectionName)
        {
            this.hybridConnectionString = hybridConnectionString;
            this.hybridConnectionName = hybridConnectionName;
        }
        
        public HybridConnectionListener CreateHybridListener(string listenerConnectionString, string hybridConnectionName)
        {
            RelayConnectionStringBuilder connectionStringBuilder = new RelayConnectionStringBuilder(listenerConnectionString) { EntityPath = hybridConnectionName };
            //var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SharedAccessKeyName, connectionStringBuilder.SharedAccessKey);
            //var uri = new Uri(string.Format("https://{0}/{1}", connectionStringBuilder.Endpoint.Host, hybridConnectionName));
            listener = new HybridConnectionListener(connectionStringBuilder.ToString());

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { System.Diagnostics.Debug.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { System.Diagnostics.Debug.WriteLine("Offline"); };
            listener.Online += (o, e) => { System.Diagnostics.Debug.WriteLine("Online"); };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                //TODO if string then handle response
                IActorMessage msg = null;
                var relay = new RecieveResponseFromRequest<MsgType>(listenerConnectionString, hybridConnectionName);
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
            throw new NotImplementedException();
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

            foreach ( var switchBoard in this.Switchboards)
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
