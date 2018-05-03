using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class SendStringActorCanOnlySendStringMessagesException : Exception
    {
        public SendStringActorCanOnlySendStringMessagesException() { }
        public SendStringActorCanOnlySendStringMessagesException(string message) : base(message) { }
        public SendStringActorCanOnlySendStringMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SendStringActorCanOnlySendStringMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
