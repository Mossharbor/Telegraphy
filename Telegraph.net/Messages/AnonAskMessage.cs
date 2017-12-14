using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    internal class AnonAskMessage<T> : SimpleMessage<T>, IActorMessage, IWrappedMessage
    {
        public AnonAskMessage(T wrappedMessage, TaskCompletionSource<IActorMessage> task):base(wrappedMessage)
        {
            this.Status = task;
        }

        public static explicit operator T(AnonAskMessage<T> msg)
        {
            return msg.OriginalMessage;
        }
    }
}
