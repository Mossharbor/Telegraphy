using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Threading;
    using System.Collections.Concurrent;

    public class LocalOperator : IOperator
    {
        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private Semaphore _dataExists = new Semaphore(0, int.MaxValue);
        ConcurrentQueue<IActorMessage> actorMessages = new ConcurrentQueue<IActorMessage>();

        public LocalOperator() :this(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors))
        { }

        public LocalOperator(LocalConcurrencyType concurrencyType, uint concurrency = 1) : this (new LocalSwitchboard(concurrencyType, concurrency))
        {}

        public LocalOperator(ILocalSwitchboard switchBoard)
        {
            this.Switchboard = switchBoard;
            this.ID = 0;
        }

        #region IActor

        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            this.NextMessage = msg;
            return true;
        }
        
        #endregion

        #region IOperator

        public ulong Count { get { return (ulong)actorMessages.Count; } }

        public long ID { get; set; }

        public void Register<T>(Action<T> action) where T : class
        {
            this.Switchboard.Register<T>(action);
        }

        public void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
            //where T : IActorMessage
            where K : IActor
        {
            this.Switchboard.Register<T, K>(factory);
        }

        public bool IsAlive()
        {
            return this.Switchboard.IsDisabled();
        }

        public void Kill()
        {
            try { _dataExists.Release((int)this.Switchboard.Concurrency); }
            catch (SemaphoreFullException) { }
            
            this.Switchboard.Disable();
        }


        public virtual void AddMessage(IActorMessage msg)
        {
            this.NextMessage = msg;
        }

        public virtual IActorMessage GetMessage()
        {
            return NextMessage;
        }
        
        public virtual bool WaitTillEmpty(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while (!actorMessages.IsEmpty)
            {
                System.Threading.Thread.Sleep(1000);

                if ((DateTime.Now - start)>timeout)
                    return false;
            }

            return true;
        }

        private ILocalSwitchboard _switchboard = null;
        public virtual ILocalSwitchboard Switchboard
        {
            get { return _switchboard; }
            set 
            {
                if (null != _switchboard)
                {
                    _switchboard.Disable();
                }

                _switchboard = value;
                _switchboard.Operator = this;
            }
        }

        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            this.Switchboard.Register(exceptionType, handler);
        }
        #endregion

        private IActorMessage NextMessage
        {
            get
            {
                bool capturedLock = _dataExists.WaitOne(new TimeSpan(0, 0, 1)); // wait until there is something to process.

                IActorMessage next = null;
                if (!actorMessages.TryDequeue(out next))
                {
                    return null;
                }
                return next;
            }
            set
            {
                actorMessages.Enqueue(value);
                try { _dataExists.Release(); }
                catch (SemaphoreFullException) { }
            }
        }

    }
}
