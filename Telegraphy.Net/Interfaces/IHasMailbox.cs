using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Collections.Concurrent;

    public interface IHasMailbox
    {
        ConcurrentQueue<IActorMessage> MessageQueue { get; }

        void SignalNewMessage();
    }
}
