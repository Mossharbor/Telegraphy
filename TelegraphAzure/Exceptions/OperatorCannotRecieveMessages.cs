using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class OperatorCannotRecieveMessagesException : Exception
    {
        public OperatorCannotRecieveMessagesException() { }
        public OperatorCannotRecieveMessagesException(string message) : base(message) { }
        public OperatorCannotRecieveMessagesException(string message, Exception inner) : base(message, inner) { }
        protected OperatorCannotRecieveMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
