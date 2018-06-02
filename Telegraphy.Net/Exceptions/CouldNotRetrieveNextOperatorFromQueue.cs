using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class CouldNotRetrieveNextOperatorFromQueueException : Exception
    {
        public CouldNotRetrieveNextOperatorFromQueueException() { }
        public CouldNotRetrieveNextOperatorFromQueueException(string message) : base(message) { }
        public CouldNotRetrieveNextOperatorFromQueueException(string message, Exception inner) : base(message, inner) { }
        protected CouldNotRetrieveNextOperatorFromQueueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
