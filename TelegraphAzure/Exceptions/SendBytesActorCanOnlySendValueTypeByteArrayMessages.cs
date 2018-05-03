using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class SendBytesActorCanOnlySendValueTypeByteArrayMessagesException : Exception
    {
        public SendBytesActorCanOnlySendValueTypeByteArrayMessagesException() { }
        public SendBytesActorCanOnlySendValueTypeByteArrayMessagesException(string message) : base(message) { }
        public SendBytesActorCanOnlySendValueTypeByteArrayMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SendBytesActorCanOnlySendValueTypeByteArrayMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
