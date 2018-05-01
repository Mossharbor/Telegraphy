using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class OperatorCannotSendMessagesException : Exception
    {
        public OperatorCannotSendMessagesException() { }
        public OperatorCannotSendMessagesException(string message) : base(message) { }
        public OperatorCannotSendMessagesException(string message, Exception inner) : base(message, inner) { }
        protected OperatorCannotSendMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
