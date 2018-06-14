using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Threading;
    using System.Collections.Concurrent;
    
    /// <summary>
    /// Telegraphy is the long-distance transmission of textual or symbolic (as opposed to verbal or audio) messages without the physical exchange of an object bearing the message
    /// </summary>
    public sealed class Telegraph : IActor
    {
        static readonly Random rand = new Random((int)DateTime.Now.Ticks);
        long operatorCount = 0;
        static long lastOperatorID = 0;
        ConcurrentDictionary<long, IOperator> operators = new ConcurrentDictionary<long, IOperator>();
        private ConcurrentDictionary<Type, IProducerConsumerCollection<IOperator>> msgTypeToOperator = new ConcurrentDictionary<Type, IProducerConsumerCollection<IOperator>>();

        private static readonly Telegraph instance = new Telegraph();

        private Telegraph() 
        {
        }

        /// <summary>
        /// Default static instance of this class
        /// </summary>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ff650316.aspx"/>
        /// <remarks>In this strategy, the instance is created the first time any member of the class is referenced. The common language runtime takes care of the variable initialization. The class is marked sealed to prevent derivation, which could add instances. For a discussion of the pros and cons of marking a class sealed, see [Sells03]. In addition, the variable is marked readonly, which means that it can be assigned only during static initialization (which is shown here) or in a class constructor. This implementation is similar to the preceding example, except that it relies on the common language runtime to initialize the variable. It still addresses the two basic problems that the Singleton pattern is trying to solve: global access and instantiation control. The public static property provides a global access point to the instance. Also, because the constructor is private, the Singleton class cannot be instantiated outside of the class itself; therefore, the variable refers to the only instance that can exist in the system. Because the Singleton instance is referenced by a private static member variable, the instantiation does not occur until the class is first referenced by a call to the Instance property. This solution therefore implements a form of the lazy instantiation property, as in the Design Patterns form of Singleton.The only potential downside of this approach is that you have less control over the mechanics of the instantiation. In the Design Patterns form, you were able to use a nondefault constructor or perform other tasks before the instantiation. Because the .NET Framework performs the initialization in this solution, you do not have these options. In most cases, static initialization is the preferred approach for implementing a Singleton in .NET</remarks>
        public static Telegraph Instance
        {
            get
            {
                return instance;
            }
        }

        private IOperator mainOperator = null;
        public IOperator MainOperator
        {
            get 
            {
                if (null == mainOperator)
                    Register(new LocalQueueOperator());

                return mainOperator; 
            }
            private set
            {
                if (null != mainOperator)
                {
                    if (!IsUsingSingleOperator())
                        throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();

                    mainOperator.Kill();
                    operators.Clear();
                    Interlocked.Decrement(ref operatorCount);
                    Register(value);
                }

                mainOperator = value;
            }
        }

        public bool IsUsingSingleOperator()
        {
            return (1 >= Interlocked.Read(ref operatorCount));
        }

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            if (null == operators)
                return true;

            DateTime start = DateTime.Now;
            //foreach (var t in operators)
            for (int i = operators.Values.Count - 1; i >= 0 && i < operators.Values.Count; --i)
            {
                IOperator t = operators.Values.ElementAt(i);

                bool timedOut = t.WaitTillEmpty(timeout);
                TimeSpan timeTakenSoFar = (DateTime.Now - start);

                if (!timedOut || timeTakenSoFar > timeout)
                    return false;
            }

            return true;
        }

        private MessageDispatchProcedureType messageDispatchProcedure = MessageDispatchProcedureType.RoundRobin;
        public MessageDispatchProcedureType MessageDispatchProcedure
        {
            get { return messageDispatchProcedure; }
            set 
            {
                if (0 != operators.Count)
                    throw new DispatchMethodCanOnlyBeSetBeforeOperatorsAreRegisteredException();

                messageDispatchProcedure = value; 
            }
        }

        #region IActor
        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            if (msg is ControlMessages.HangUp)
            {
                Broadcast(msg);
                UnRegisterAll();
                msg.Status?.TrySetResult(msg);
                return true;
            }

            if (IsUsingSingleOperator())
                return MainOperator.OnMessageRecieved<T>(msg);

            IOperator op = null;
            switch (this.MessageDispatchProcedure)
            {
                case MessageDispatchProcedureType.RandomSelection:
                    {
                        var handlesType = typeof(T);

                        if (!msgTypeToOperator.ContainsKey(handlesType))
                            throw new NoOperatorRegisteredToSupportTypeException();

                        int nextIndex = rand.Next(0, msgTypeToOperator[handlesType].Count-1);

                        op = msgTypeToOperator[handlesType].ElementAt(nextIndex);
                    }
                    break;

                case MessageDispatchProcedureType.BroadcastToAll:
                    return Broadcast<T>(msg);

                case MessageDispatchProcedureType.RoundRobin:
                    {
                        var handlesType = msg.GetType();

                        if (!msgTypeToOperator.ContainsKey(handlesType))
                            throw new NoOperatorRegisteredToSupportTypeException();
                        if (((ConcurrentQueue<IOperator>)msgTypeToOperator[handlesType]).Count == 1)
                        {
                            if (!((ConcurrentQueue<IOperator>)msgTypeToOperator[handlesType]).TryPeek(out op))
                                throw new CouldNotRetrieveNextOperatorFromQueueException();
                        }
                        else
                        {
                            if (!((ConcurrentQueue<IOperator>)msgTypeToOperator[handlesType]).TryDequeue(out op))
                                throw new CouldNotRetrieveNextOperatorFromQueueException();

                            //Re-Add the operator to the top of the queue.
                            ((ConcurrentQueue<IOperator>)msgTypeToOperator[handlesType]).Enqueue(op);
                        }
                    }
                    break;
                case MessageDispatchProcedureType.LeastBusy:
                    {
                        var handlesType = typeof(T);

                        if (!msgTypeToOperator.ContainsKey(handlesType))
                            throw new NoOperatorRegisteredToSupportTypeException();

                        ulong leastCount = ulong.MaxValue;
                        for (int i = msgTypeToOperator[handlesType].Count - 1; i >= 0 && i < msgTypeToOperator[handlesType].Count; --i)
                        {
                            IOperator tempOp = msgTypeToOperator[handlesType].ElementAt(i);

                            if (leastCount > tempOp.Count)
                            {
                                leastCount = tempOp.Count;
                                op = tempOp;
                            }

                            if (0 == tempOp.Count)
                                break;
                        }
                    }
                    break;
                default:
                    throw new UnsupportedDispatchImplementationException();
            }

            return op.OnMessageRecieved<T>(msg);
        }

        public bool Broadcast<T>(T message) where T : class
        {
            IActorMessage msg = null;
            var handlesType = typeof(T);

            if (!(message is IActorMessage))
                msg = new SimpleMessage<T>(message);
            else
                msg = (message as IActorMessage);

            if((msg is ControlMessages.HangUp))
            {
                foreach (var t in msgTypeToOperator)
                    foreach (var q in t.Value)
                        q.OnMessageRecieved(msg);
                return true;
            }

            if (!msgTypeToOperator.ContainsKey(handlesType))
            {
                if (!IsUsingSingleOperator())
                    throw new NoOperatorRegisteredToSupportTypeException();

                return MainOperator.OnMessageRecieved(msg);
            }

            bool recieved = true;
            for (int i = msgTypeToOperator[handlesType].Count - 1; i >= 0 && i < msgTypeToOperator[handlesType].Count; --i)
            {
                var t = msgTypeToOperator[handlesType].ElementAt(i);
                recieved &= t.OnMessageRecieved(msg);
            }

            return recieved;
        }

        #endregion

        #region Registration

        private long GetNextOperatorID()
        {
            Interlocked.Increment(ref operatorCount);
            return Interlocked.Increment(ref lastOperatorID);
        }

        public void UnRegisterAll()
        {
            if (null != operators)
            {
                operatorCount = 0;
                operators.Clear();
            }

            if (null != mainOperator)
                mainOperator = null;

            if (null != msgTypeToOperator)
                msgTypeToOperator.Clear();
        }

        public void Register<T>(Action<T> action) where T : class
        {
            if (IsUsingSingleOperator())
            {
                if (0 == this.MainOperator.Switchboards.Count)
                    throw new CannotRegisterActionSinceOperatorHasNoSwitchBoardsException();

                foreach ( var switchboard in this.MainOperator.Switchboards)
                    switchboard.Register<T>(action);
            }
            else
                throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();
        }

        public void Register<MsgType, ActorType>(System.Linq.Expressions.Expression<Func<ActorType>> factory)
            where MsgType : class
            where ActorType : IActor
        {
            if (IsUsingSingleOperator())
            {
                if(0 == this.MainOperator.Switchboards.Count)
                    throw new CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException();

                foreach (var switchboard in this.MainOperator.Switchboards)
                    switchboard.Register<MsgType, ActorType>(factory);
            }
            else
                throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            if (IsUsingSingleOperator())
            {
                if(0 == this.MainOperator.Switchboards.Count)
                    throw new CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException();

                foreach (var switchboard in this.MainOperator.Switchboards)
                    switchboard.Register(exceptionType, handler);
            }
            else
                throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();
        }

        public long Register(IOperator op)
        {
            if (op.ID <= 0)
            {
                op.ID = GetNextOperatorID();

                if (!operators.TryAdd(op.ID, op))
                    throw new FailedToRegisterOperatorForNewIdException(op.ID.ToString());
            }
            else if (!operators.ContainsKey(op.ID))
            {
                if (!operators.TryAdd(op.ID, op))
                    throw new FailedToRegisterOperatorForExistingIdException(op.ID.ToString());
            }

            if (null == mainOperator)
                mainOperator = op;

            return op.ID;
        }

        public long Register<T>(IOperator op, string registrationString)
        {
            foreach (var switchBoard in op.Switchboards)
            {
                if (String.IsNullOrEmpty(registrationString))
                    switchBoard.Register<T>();
                else
                    switchBoard.Register<T>(registrationString);
            }

            return Register<T>(op, false);
        }

        public long Register<T>(IOperator op)
        {
            return Register<T>(op, true);
        }

        private long Register<T>(IOperator op, bool registerSwitchBoard = true)
        {
            long opID = Register(op);

            if (registerSwitchBoard)
            {
                foreach (var switchBoard in op.Switchboards)
                    switchBoard.Register<T>();
            }

            var handlesType = typeof(T);

            if (!msgTypeToOperator.ContainsKey(handlesType))
            {
                if (this.MessageDispatchProcedure == MessageDispatchProcedureType.RoundRobin)
                {
                    if (!msgTypeToOperator.TryAdd(handlesType, new ConcurrentQueue<IOperator>()))
                        throw new FailedToRegisterOperatorForTypeException(this.MessageDispatchProcedure.ToString() + ":" + handlesType.ToString());
                }
                else
                {
                    if (!msgTypeToOperator.TryAdd(handlesType, new ConcurrentBag<IOperator>()))
                        throw new FailedToRegisterOperatorForTypeException(this.MessageDispatchProcedure.ToString() + ":" + handlesType.ToString());
                }
            }

            if (this.MessageDispatchProcedure == MessageDispatchProcedureType.RoundRobin)
            {
                ((ConcurrentQueue<IOperator>)msgTypeToOperator[handlesType]).Enqueue(op);
            }
            else
            {
                if (msgTypeToOperator[handlesType].TryAdd(op))
                    throw new FailedToRegisterOperatorForTypeException(handlesType.ToString());
            }

            return opID;
        }
        
        public void Register<T>(long opId)
        {
            IOperator op = null;
            if (!operators.TryGetValue(opId, out op))
                throw new NoOperatorFoundForIDException();
            Register<T>(op, true);
        }

        public void Register<T>(long opID, Action<T> action) where T : class
        {
            IOperator op = null;
            if (!operators.TryGetValue(opID, out op))
                throw new NoOperatorFoundForIDException();

            this.Register<T>(op,action);
        }

        public long Register<T>(IOperator op, Action<T> action) where T : class
        {
            if (0 == op.Switchboards.Count)
                throw new CannotRegisterActionSinceOperatorHasNoSwitchBoardsException();
            long nextID = Register(op);
            foreach(var switchBoard in op.Switchboards)
                switchBoard.Register<T>(action);
            Register<T>(op, false);
            return nextID;
        }

        public void Register<T, K>(long opID, System.Linq.Expressions.Expression<Func<K>> factory)
            where T : class
            where K : IActor
        {
            IOperator op = null;
            if (!operators.TryGetValue(opID, out op))
                throw new NoOperatorFoundForIDException();

            Register<T, K>(op, factory);
        }

        public long Register<T, K>(IOperator op, System.Linq.Expressions.Expression<Func<K>> factory)
            where T : class
            where K : IActor
        {
            if (0 == op.Switchboards.Count)
                throw new CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException();

            long nextID = Register(op);

            foreach (var switchboard in op.Switchboards)
                switchboard.Register<T, K>(factory);
            Register<T>(op, false);
            return nextID;
        }

        public void Register(long opID, Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            IOperator op = null;
            if (!operators.TryGetValue(opID, out op))
                throw new NoOperatorFoundForIDException();

            Register(op, exceptionType, handler);
        }

        public long Register(IOperator op, Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            long nextID = Register(op);

            if (0 == op.Switchboards.Count)
                throw new CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException();

            foreach (var switchboard in op.Switchboards)
                switchboard.Register(exceptionType, handler);
            return nextID;
        }

        #endregion
    }
}
