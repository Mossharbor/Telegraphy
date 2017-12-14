using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedMessageEnqueException : Exception
    {
        public FailedMessageEnqueException() { }
        public FailedMessageEnqueException(string message) : base(message) { }
        public FailedMessageEnqueException(string message, Exception inner) : base(message, inner) { }
        protected FailedMessageEnqueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
