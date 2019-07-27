using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net.Exceptions
{

    [Serializable]
    public class UnsupportedMessageException : Exception
    {
        public UnsupportedMessageException() { }
        public UnsupportedMessageException(string message) : base(message) { }
        public UnsupportedMessageException(string message, Exception inner) : base(message, inner) { }
        protected UnsupportedMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
