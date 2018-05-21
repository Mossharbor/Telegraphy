using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Threading;
    using System.Collections.Concurrent;

    public class MailBoxActor : LocalSwitchboard, IHasMailbox, IActor, IWrappedActor
    {
        private Semaphore _dataExists = new Semaphore(0, int.MaxValue);
        IActor wrappedActor;
        public MailBoxActor(IActor actorToWrap) : base(LocalConcurrencyType.OneThreadAllActors)
        {
            this.wrappedActor = actorToWrap;
            this.MessageQueue = new ConcurrentQueue<IActorMessage>();
            SpawnThreads();
            _actorRegistered.Set();
        }

        public IActor OriginalActor { get { return wrappedActor; } }

        private ConcurrentQueue<IActorMessage> messageQueue;
        public ConcurrentQueue<IActorMessage> MessageQueue
        {
            get 
            {
                return messageQueue; 
            }
            private set { messageQueue = value; }
        }

        public void SignalNewMessage()
        {
            try { _dataExists.Release(); }
            catch (SemaphoreFullException) { }
        }

        protected virtual void OnExiting()
        {
            // do nothing by default
        }

        protected override IActor GetActorForMessage(IActorMessage msg)
        {
            return this.OriginalActor;
        }

        protected override IActorMessage GetNextMessage()
        {
            bool capturedLock = _dataExists.WaitOne(new TimeSpan(0, 0, 1)); // wait until there is something to process.

            IActorMessage next = null;
            if (!MessageQueue.TryDequeue(out next))
            {
                return null;
            }

            return next;
        }

        #region IActor
        public override bool OnMessageRecieved<T>(T msg)
        {
            MessageQueue.Enqueue(msg);

            SignalNewMessage();

            return true;
        }

        public override void Register<T>(Action<T> action)
        {
            throw new NotImplementedException();
        }

        public override void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
