using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.IO.Exceptions
{

    [Serializable]
    public class CannotSendNonStreamMessagesToFileException : Exception
    {
        public CannotSendNonStreamMessagesToFileException() { }
        public CannotSendNonStreamMessagesToFileException(string message) : base(message) { }
        public CannotSendNonStreamMessagesToFileException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendNonStreamMessagesToFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
