using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class FailedToInvokeMessageTypeException : Exception
    {
        public FailedToInvokeMessageTypeException() { }
        public FailedToInvokeMessageTypeException(string message) : base(message) { }
        public FailedToInvokeMessageTypeException(string message, Exception inner) : base(message, inner) { }
        protected FailedToInvokeMessageTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
