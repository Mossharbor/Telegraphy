using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.IO.Exceptions
{

    [Serializable]
    public class CannotSendNonStringMessagesToFileException : Exception
    {
        public CannotSendNonStringMessagesToFileException() { }
        public CannotSendNonStringMessagesToFileException(string message) : base(message) { }
        public CannotSendNonStringMessagesToFileException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendNonStringMessagesToFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
