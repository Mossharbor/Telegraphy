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
}
