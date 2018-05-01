using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    internal class ActorMessageInvocation<K> : ActorMessageInvocationBase, IActorMessageIdentifier where K : IActorMessage
    {
        private readonly Func<object, K> invoker;

        internal ActorMessageInvocation(Func<object, K> invoker)
        {
            this.invoker = invoker;
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; private set; }

        internal override IActorMessage Invoke(object message)
        {
            return invoker.Invoke(message);
        }
    }
}
