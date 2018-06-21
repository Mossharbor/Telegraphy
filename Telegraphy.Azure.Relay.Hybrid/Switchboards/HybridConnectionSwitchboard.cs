using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Microsoft.Azure.Relay;
using System.Threading;
using System.Net;
using System.IO;

namespace Telegraphy.Azure.Relay.Hybrid
{
    public class HybridConnectionSwitchboard : ILocalSwitchboard
    {
        private ConcurrentDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>>();
        private ConcurrentDictionary<Type, string> messageTypeToConnectionString = new ConcurrentDictionary<Type, string>();
        private string connectionString = null;
        private string defaultRelayName = null;
        protected long tellingCount = 0;
        protected long threadExitFlag = 0;
        protected long exitedFlag = 0;
        static object _threadAllocationLock = new object();
        private List<Thread> _concurrentThreadList = new List<Thread>();
        protected AutoResetEvent _typeRegistered = new AutoResetEvent(false);

        public HybridConnectionSwitchboard(uint concurency, string connectionString)
            : this(concurency, connectionString, new RelayConnectionStringBuilder(connectionString).EntityPath)
        {
        }

        public HybridConnectionSwitchboard(uint concurency, string connectionString, string relayName)
        {
            this.Concurrency = concurency;
            this.LocalConcurrencyType = LocalConcurrencyType.DedicatedThreadCount;
            this.connectionString = connectionString;
            this.defaultRelayName = relayName;
        }

        private bool ShouldExit()
        {
            bool exit = (1 == Interlocked.Read(ref threadExitFlag));
            return exit;
        }

        private void SpawnThreads()
        {
            for (int i = 0; i < this.Concurrency; ++i)
            {
                ThreadStart start = new ThreadStart(ProcessIActorMessageMessagePump);
                Thread thrd = new Thread(start);
                thrd.IsBackground = true;
                thrd.Name = "ProcessIActorMessagePumpWorker" + i.ToString();

                _concurrentThreadList.Add(thrd);
                thrd.Start();
                System.Diagnostics.Debug.WriteLine("Creating " + this.LocalConcurrencyType + " Thread.");
            }
        }

        protected virtual IActorMessage GetNextMessage()
        {
            // TODO listen to the open relay for messages as well
            // and return them if they come in.
            return this.Operator.GetMessage();
        }

        private void ProcessIActorMessageMessagePump()
        {
            IActorMessage msg = null;

            _typeRegistered.WaitOne();
            _typeRegistered.Set();

            for (; ; )
            {
                msg = GetNextMessage();

                if (ShouldExit())
                    return;

                if (null == msg)
                    continue;

                if (typeof(ControlMessages.HangUp) == msg.GetType())
                {
                    if (null != msg.Status && !msg.Status.Task.IsCompleted)
                        msg.Status.SetResult(msg);
                    return;
                }
                try
                {
                    string relayName = messageTypeToConnectionString[msg.GetType()];
                    IActor actor = new RecieveResponseFromRequestByType(msg.GetType(), msg.GetType(), connectionString, relayName);

                    Interlocked.Increment(ref tellingCount);
                    Type msgType = msg.GetType();
                    actor.OnMessageRecieved(msg);
                }
                catch (Exception ex)
                {
                    //TODO CheckForAndRunExceptionHandler(ex, actor, msg);
                }
            }
        }

        #region ILocalSwitchboard
        public uint Concurrency { get; private set; }

        public LocalConcurrencyType LocalConcurrencyType { get; private set; }

        private IOperator myOperator = null;
        public IOperator Operator
        {
            get { return myOperator; }
            set
            {
                if (0 == Interlocked.Read(ref tellingCount))
                {
                    myOperator = value;
                    SpawnThreads();
                }
                else
                    throw new CannotSwitchOperatorsOnExecutingSwitchboardException();
            }
        }

        public void Disable()
        {
            Interlocked.Increment(ref threadExitFlag);
            lock (_threadAllocationLock)
            {
                _typeRegistered.Set();

                foreach (var thread in _concurrentThreadList)
                {
                    thread.Join();
                    System.Diagnostics.Debug.WriteLine("Exiting " + this.LocalConcurrencyType + " Thread.");
                }

                _concurrentThreadList.Clear();
                tellingCount = 0;
            }
            System.Diagnostics.Debug.WriteLine("Disabled");
            Interlocked.Decrement(ref threadExitFlag);
        }

        public bool IsDisabled()
        {
            bool exit = (1 == Interlocked.Read(ref exitedFlag));
            return exit;
        }

        public void Register<T>(Action<T> action) where T : class
        {
            throw new SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException();
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
            throw new SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException();
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            _exceptionTypeToHandler.AddOrUpdate(exceptionType, handler, (key, oldValue) => handler);
        }

        public void Register<T>()
        {
            Register<T>(defaultRelayName);
        }

        public void Register<T>(string relayName)
        {
            _typeRegistered.Set();
            if (!messageTypeToConnectionString.TryAdd(typeof(T), relayName))
            {
                if (!messageTypeToConnectionString.ContainsKey(typeof(T)))
                    throw new FailedToRegisterActionForTypeException(typeof(T).ToString());
                else if (!messageTypeToConnectionString[typeof(T)].Equals(relayName))
                    throw new FailedToRegisterActionForTypeException(typeof(T).ToString());
            }
        }
        #endregion
        #region IActor
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (null == this.Operator)
                return false;

            this.Operator.AddMessage(msg);
            return true;
        }
        #endregion

    }
}
