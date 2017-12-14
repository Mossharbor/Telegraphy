﻿using System;
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
                    Register(new LocalOperator());

                return mainOperator; 
            }
            set
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
        public bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
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
                                throw new NotImplementedException(); //TODO:
                        }
                        else
                        {
                            if (!((ConcurrentQueue<IOperator>)msgTypeToOperator[handlesType]).TryDequeue(out op))
                                throw new NotImplementedException(); // TODO:

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

        public bool Broadcast<T>(T message)
        {
            IActorMessage msg = null;
            var handlesType = typeof(T);

            if (!(message is IActorMessage))
                msg = new SimpleMessage<T>(message);
            else
                msg = (message as IActorMessage);

            if (!msgTypeToOperator.ContainsKey(handlesType))
                throw new NoOperatorRegisteredToSupportTypeException();

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

        public void Register<T>(Action<T> action)
        {
            if (IsUsingSingleOperator())
                this.MainOperator.Register<T>(action);
            else
                throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();
        }

        public void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
            where K : IActor
        {
            if (IsUsingSingleOperator())
                this.MainOperator.Register<T, K>(factory);
            else
                throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            if (IsUsingSingleOperator())
                this.MainOperator.Register(exceptionType, handler);
            else
                throw new FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException();
        }

        public long Register(IOperator op)
        {
            if (op.ID <= 0)
            {
                op.ID = GetNextOperatorID();

                if (!operators.TryAdd(op.ID, op))
                    throw new FailedToRegisterOperatorException("Failed To Register Operator for new ID"+op.ID);
            }
            else if (!operators.ContainsKey(op.ID))
            {
                if (!operators.TryAdd(op.ID, op))
                    throw new FailedToRegisterOperatorException("Failed to Register Operator for existing ID"+op.ID);
            }

            if (null == mainOperator)
                mainOperator = op;

            return op.ID;
        }

        public void Register<T>(IOperator op)
        {
            Register(op);

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
        }

        public void Register<T>(long opID, Action<T> action)
        {
            IOperator op = null;
            if (!operators.TryGetValue(opID, out op))
                throw new NoOperatorFoundForIDException();

            this.Register<T>(op,action);
        }

        public long Register<T>(IOperator op, Action<T> action)
        {
            long nextID = Register(op);
            op.Register<T>(action);
            Register<T>(op);
            return nextID;
        }

        public void Register<T, K>(long opID, System.Linq.Expressions.Expression<Func<K>> factory)
            where K : IActor
        {
            IOperator op = null;
            if (!operators.TryGetValue(opID, out op))
                throw new NoOperatorFoundForIDException();

            Register<T, K>(op, factory);
        }

        public long Register<T, K>(IOperator op, System.Linq.Expressions.Expression<Func<K>> factory)
            where K : IActor
        {
            long nextID = Register(op);
            op.Register<T, K>(factory);
            Register<T>(op);
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
            op.Register(exceptionType, handler);
            return nextID;
        }

        #endregion
    }
}
