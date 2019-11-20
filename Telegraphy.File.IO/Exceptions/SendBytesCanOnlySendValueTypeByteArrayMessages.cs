using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.File.IO.Exceptions
{

    [Serializable]
    public class SendBytesCanOnlySendValueTypeByteArrayMessagesException : Exception
    {
        public SendBytesCanOnlySendValueTypeByteArrayMessagesException() { }
        public SendBytesCanOnlySendValueTypeByteArrayMessagesException(string message) : base(message) { }
        public SendBytesCanOnlySendValueTypeByteArrayMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SendBytesCanOnlySendValueTypeByteArrayMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
