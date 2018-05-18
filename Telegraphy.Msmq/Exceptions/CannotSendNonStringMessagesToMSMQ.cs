using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Msmq.Exceptions
{

    [Serializable]
    public class CannotSendNonStringMessagesToMSMQException : Exception
    {
        public CannotSendNonStringMessagesToMSMQException() { }
        public CannotSendNonStringMessagesToMSMQException(string message) : base(message) { }
        public CannotSendNonStringMessagesToMSMQException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendNonStringMessagesToMSMQException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
