using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    internal abstract class ActorMessageInvocationBase
    {
        internal abstract IActorMessage Invoke(object message);
    }

    internal class ActorMessageInvocation<K>: ActorMessageInvocationBase where K : IActorMessage
    {
        private readonly Func<object, K> invoker;

        internal ActorMessageInvocation(Func<object,K> invoker)
        {
            this.invoker = invoker;
        }

        internal override IActorMessage Invoke(object message)
        {
            return invoker.Invoke(message);
        }
    }
}
