using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class SendBytesStorageActorCanOnlySendValueTypeByteArrayMessagesException : Exception
    {
        public SendBytesStorageActorCanOnlySendValueTypeByteArrayMessagesException() { }
        public SendBytesStorageActorCanOnlySendValueTypeByteArrayMessagesException(string message) : base(message) { }
        public SendBytesStorageActorCanOnlySendValueTypeByteArrayMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SendBytesStorageActorCanOnlySendValueTypeByteArrayMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
