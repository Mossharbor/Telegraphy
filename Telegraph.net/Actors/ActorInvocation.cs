using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Collections.Concurrent;
    using System.Threading;

    public abstract class ActorInvocationBase : IActor, IActorInvocation
    {
        protected bool withMailbox = false;
        private Semaphore _dataExists = new Semaphore(0, int.MaxValue);

        ConcurrentQueue<IActorMessage> actorMailbox = new ConcurrentQueue<IActorMessage>();

        internal abstract IActor Invoke(bool withMailbox);

        public abstract void Reset();

        public IActor Invoke()
        {
            return this.Invoke(withMailbox);
        }

        public IActorMessage MessageToProcess { get; set; }

        public Type InvokesType { get; set; }

        public bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
            actorMailbox.Enqueue(msg);
            return true;
        }

        public void Register<T>(Action<T> action)
        {
            Telegraph.Instance.Register<T>(action);
        }

        public void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
                //where T : IActorMessage
                where K : IActor
        {
            Telegraph.Instance.Register<T, K>(factory);
        }
    }

    internal class ActorInvocation<K> : ActorInvocationBase where K : IActor
    {
        private readonly Func<K> invoker;
        private IActor prevInvokedActor = null;

        internal ActorInvocation(Func<K> invoker)
        {
            this.InvokesType = null;
            this.invoker = invoker;
        }

        public override void Reset()
        {
            this.prevInvokedActor = null;
        }

        internal override IActor Invoke(bool withMailbox)
        {
            if (null != prevInvokedActor)
                return prevInvokedActor;

            this.withMailbox = withMailbox;
            var t = invoker.Invoke();

            InvokesType = t.GetType();
            System.Diagnostics.Debug.WriteLine("Instantiating "+InvokesType.ToString() + ((withMailbox)?" with Mailbox":""));

            //if (t is IAnonActor)
            //{
            //    prevInvokedActor = null;
            //    return t;
            //}

            if (withMailbox && !(t is IHasMailbox))
            {
                prevInvokedActor = new MailBoxActor(t);
            }
            else
            {
                prevInvokedActor = t;
            }
            return prevInvokedActor;
        }
    }
}
