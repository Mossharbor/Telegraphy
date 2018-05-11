using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class MessageTypeWasNotAFileNameCannotDeleteBlobDataException : Exception
    {
        public MessageTypeWasNotAFileNameCannotDeleteBlobDataException() { }
        public MessageTypeWasNotAFileNameCannotDeleteBlobDataException(string message) : base(message) { }
        public MessageTypeWasNotAFileNameCannotDeleteBlobDataException(string message, Exception inner) : base(message, inner) { }
        protected MessageTypeWasNotAFileNameCannotDeleteBlobDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
