using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Msmq.Exceptions
{

    [Serializable]
    public class SendBytesMsMessageQueueCanOnlySendValueTypeByteArrayMessagesException : Exception
    {
        public SendBytesMsMessageQueueCanOnlySendValueTypeByteArrayMessagesException() { }
        public SendBytesMsMessageQueueCanOnlySendValueTypeByteArrayMessagesException(string message) : base(message) { }
        public SendBytesMsMessageQueueCanOnlySendValueTypeByteArrayMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SendBytesMsMessageQueueCanOnlySendValueTypeByteArrayMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
