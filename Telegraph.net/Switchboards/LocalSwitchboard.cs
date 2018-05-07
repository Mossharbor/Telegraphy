using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Linq.Expressions;

    public class LocalSwitchboard : IActor, ILocalSwitchboard
    {
        //TODO use Lazy<IActor for actor instantiation
        private ConcurrentDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>>();
        private ConcurrentDictionary<Type, ActorInvocationBase> _actorTypeToInstatiation = new ConcurrentDictionary<Type, ActorInvocationBase>();
        private ConcurrentDictionary<Type, IActor> _messageTypeToActor = new ConcurrentDictionary<Type, IActor>();
        private List<Thread> _concurrentThreadList = new List<Thread>();
        private Semaphore _concurrentThreadLock = null;
        protected Semaphore _actorRegistered = new Semaphore(0, int.MaxValue);
        protected long tellingCount = 0;
        protected long threadExitFlag = 0;
        protected long exitedFlag = 0;
        private static uint DefaultConcurrency = 3;
        static object _threadAllocationLock = new object();

        public LocalSwitchboard(LocalConcurrencyType concurrencyType)
            : this(concurrencyType, (LocalConcurrencyType.ActorsOnThreadPool != concurrencyType) ? (uint)DefaultConcurrency : (uint)System.Environment.ProcessorCount - 1)
        { }

        public LocalSwitchboard(LocalConcurrencyType concurrencyType, uint concurrency)
        {
            this.LocalConcurrencyType = concurrencyType;
            this.Concurrency = concurrency;

            if(0 != Concurrency)
                _concurrentThreadLock = new Semaphore((int)Concurrency, (int)Concurrency);
        }

        #region IActor
        public virtual bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            if (null == this.Operator)
                return false;

            this.Operator.AddMessage(msg);
            return true;
        }

        public virtual void Register<T>(Action<T> action) where T: class
        {
            var handlesType = typeof(T);
            bool decorateActorWithMailbox = LocalConcurrencyType.OneThreadPerActor == this.LocalConcurrencyType;
            bool isManyThreads = LocalConcurrencyType.OneActorPerThread == this.LocalConcurrencyType;

            IActor anonActor = null;
            if (decorateActorWithMailbox)
                anonActor = new MailBoxActor(anonActor);
            else
                anonActor = new AnonActor<T>(action);

            // For invocation return the anonActor we just created always.
            ActorInvocationBase invoker = new ActorInvocation<IActor>(() => anonActor);

            //NOTE: for anonymouse Actions the actor type is the same as the message type
            if (!_actorTypeToInstatiation.TryAdd(handlesType, invoker))
                throw new FailedToRegisterActorInvocationForActionException(handlesType.ToString());

            if (!_messageTypeToActor.TryAdd(handlesType, anonActor))
                throw new FailedToRegisterActionForTypeException(handlesType.ToString());

            uint iterationCount = (isManyThreads) ? this.Concurrency : 1;
            try { _actorRegistered.Release((int)iterationCount); }
            catch (SemaphoreFullException) { }
        }

        public virtual void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> actorCreationFunction)
            //where T : IActorMessage
            where K : IActor
        {
            //var handlesType = typeof(T);
            //var actor = func.Invoke();
            bool decorateActorWithMailbox = LocalConcurrencyType.OneThreadPerActor == this.LocalConcurrencyType;
            var actorType = typeof(K);
            var handlesType = typeof(T);
            IActor actor = null;
            ActorInvocationBase invoker = null;

            if (_actorTypeToInstatiation.ContainsKey(actorType))
            {
                if (!_actorTypeToInstatiation.TryGetValue(actorType, out invoker))
                    invoker = null;
                else
                {
                    actor = invoker.Invoke(decorateActorWithMailbox);
                }
            }
            
            if (null == invoker)
            {
                var func = actorCreationFunction.Compile();
                invoker = new ActorInvocation<K>(func);
                actor = invoker.Invoke(decorateActorWithMailbox);
                if (!_actorTypeToInstatiation.TryAdd(actorType, invoker))
                    throw new FailedToRegisterActorInvocationForTypeException(actorType.ToString());
            }

            if (!_messageTypeToActor.TryAdd(handlesType, actor))
                throw new FailedToRegisterActorForTypeException(handlesType.ToString());

            try { _actorRegistered.Release(); }
            catch (SemaphoreFullException) { }
        }
        #endregion

        #region ISwitchBoard
        public virtual void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            _exceptionTypeToHandler.AddOrUpdate(exceptionType, handler, (key, oldValue) => handler);

            foreach (var actor in _messageTypeToActor.Values)
            {
                if (actor is ILocalSwitchboard)
                    (actor as ILocalSwitchboard).Register(exceptionType, handler);
            }
        }

        private IOperator myOperator = null;
        public IOperator Operator
        {
            get { return myOperator; }
            set
            {
                if (0 == Interlocked.Read(ref tellingCount))
                {
                    this.Disable();
                    myOperator = value;

                    SpawnThreads();
                }
                else
                    throw new InvalidOperationException("We cannot change operators once we have started processing messages.");
            }
        }

        public bool IsDisabled()
        {
            if (ShouldExit())
                return false; // we have not exited yet

            bool exit = (1 == Interlocked.Read(ref exitedFlag));
            return exit;
        }

        public void Disable()
        {
            Interlocked.Increment(ref threadExitFlag);
            lock (_threadAllocationLock)
            {
                try
                {
                    _actorRegistered.Release((int)this.Concurrency);
                }
                catch (SemaphoreFullException) { }

                foreach (var thread in _concurrentThreadList)
                {
                    thread.Join();
                    System.Diagnostics.Debug.WriteLine("Exiting " + this.LocalConcurrencyType + " Thread.");
                }

                _concurrentThreadList.Clear();
                tellingCount = 0;
            }

            // TODO disable the actors running their own mailbox monitoring threads.

            System.Diagnostics.Debug.WriteLine("Disabled");
            Interlocked.Decrement(ref threadExitFlag);
            Interlocked.Decrement(ref exitedFlag);
        }

        public LocalConcurrencyType LocalConcurrencyType { get; private set; }

        public uint Concurrency { get; private set; }

        #endregion

        protected virtual IActor GetActorForMessage(IActorMessage msg)
        {
            IActor actor = null;
            _messageTypeToActor.TryGetValue(msg.GetType(), out actor);

            return actor;
        }

        protected virtual IActorMessage GetNextMessage()
        {
            return this.Operator.GetMessage();
        }

        private bool ShouldExit()
        {
            bool exit = (1 == Interlocked.Read(ref threadExitFlag));

            if (exit)
            {
                OnExiting();
                return true;
            }

            return false;
        }

        private void OnExiting()
        {
            bool decorateActorWithMailbox = LocalConcurrencyType.OneThreadPerActor == this.LocalConcurrencyType;
            foreach (var t in _actorTypeToInstatiation)
            {
                try
                {
                    t.Value.Invoke(decorateActorWithMailbox).OnMessageRecieved(new ControlMessages.HangUp());
                }
                catch (Exception)
                {
                    //TODO signal that the actor cannot handle the Hangup Message.
                }
            }
        }

        protected bool ProcessIActorMessage(ref IActor actor, IActorMessage msg)
        {
            System.Diagnostics.Debug.Assert(null != actor);

            if (null != msg.Status && msg.Status.Task.IsCanceled)
                return true;

            if (actor is IHasMailbox)
            {
                actor.OnMessageRecieved(msg);
            }
            else
            {
                actor.OnMessageRecieved(msg);

                if (null != msg.Status && !msg.Status.Task.IsCanceled)
                {
                    if (msg.Message is IActorMessage)
                        msg.Status.TrySetResult((IActorMessage)msg.Message);
                    else
                        msg.Status.TrySetResult(msg);
                }
            }
            
            return true;
        }

        private void ProcessIActorMessageMessagePump(object state)
        {
            bool useThreadPool = ((bool)state);
            IActorMessage msg = null;

            _actorRegistered.WaitOne();

            for(;;)
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
                    //this.Disable();
                    return;
                }

                Interlocked.Increment(ref tellingCount);

                if (useThreadPool)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessIActorMessageOnThreadPoolFcn), msg);
                }
                else
                {
                    IActor actor = null;

                    try
                    {
                        actor = GetActorForMessage(msg);

                        if (null == actor)
                            throw new NoActorForMessageTypeException();

                        ProcessIActorMessage(ref actor, msg);
                    }
                    catch (Exception ex)
                    {
                        CheckForHandler(ex, actor, msg);
                    }
                }
            }
        }

        private void ProcessIActorMessageOnThreadPoolFcn(object state)
        {
            try
            {
                if (null != _concurrentThreadLock)
                    _concurrentThreadLock.WaitOne();

                if (ShouldExit())
                    return;

                IActorMessage msg = ((IActorMessage)state);
                IActor actor = null;

                try
                {
                    actor = GetActorForMessage(msg);

                    if (null == actor)
                        throw new NoActorForMessageTypeException();

                    ProcessIActorMessage(ref actor, msg);
                }
                catch (Exception ex)
                {
                    CheckForHandler(ex, actor, msg);
                }
            }
            finally
            {
                if (null != _concurrentThreadLock)
                    _concurrentThreadLock.Release();
            }
        }

        private void ProcessMultipleIActorMessagePump()
        { 
            ProcessIActorMessageMessagePump(false);
        }

        protected void SpawnThreads()
        {
            bool startSingleThreadPump = (LocalConcurrencyType.OneThreadAllActors == this.LocalConcurrencyType || LocalConcurrencyType.ActorsOnThreadPool == this.LocalConcurrencyType || LocalConcurrencyType.OneThreadPerActor == this.LocalConcurrencyType);
            if (this.Concurrency <= 0 && !startSingleThreadPump)
                return;

            lock (_threadAllocationLock)
            {
                if (startSingleThreadPump)
                {
                    ParameterizedThreadStart start = new ParameterizedThreadStart(ProcessIActorMessageMessagePump);

                    Thread thrd = new Thread(start);
                    thrd.IsBackground = true;
                    thrd.Name = "ProcessIActorMessagePumpWorker";

                    _concurrentThreadList.Add(thrd);
                    thrd.Start((this.LocalConcurrencyType == LocalConcurrencyType.ActorsOnThreadPool));
                    System.Diagnostics.Debug.WriteLine("Creating " + this.LocalConcurrencyType + " Thread.");
                    this.Concurrency = 1;
                }
                else
                {
                    for (int i = 0; i < this.Concurrency; ++i)
                    {
                        ThreadStart start = new ThreadStart(ProcessMultipleIActorMessagePump);
                        Thread thrd = new Thread(start);
                        thrd.IsBackground = true;
                        thrd.Name = "ProcessIActorMessagePumpWorker" + i.ToString();

                        _concurrentThreadList.Add(thrd);
                        thrd.Start();
                        System.Diagnostics.Debug.WriteLine("Creating " + this.LocalConcurrencyType + " Thread.");
                    }
                }
            }

            threadExitFlag = 0;
        }

        private void CheckForHandler(Exception ex, IActor actor, IActorMessage msg)
        {
            try
            {
                Exception foundEx = null;
                Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler;

                handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, actor, msg, out foundEx);
                if (null == handler)
                    return;

                IActorInvocation invoker = null;
                IActor newActor = handler.Invoke(foundEx, actor, msg, invoker);

                if (null == newActor)
                    return;

                _messageTypeToActor.TryUpdate(msg.GetType(), newActor, actor);
            }
            catch (NotImplementedException)
            {
                System.Diagnostics.Debug.Assert(false);
                throw;
            }
            catch (Exception)
            {
                //TODO fail hard!!!
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }
}
