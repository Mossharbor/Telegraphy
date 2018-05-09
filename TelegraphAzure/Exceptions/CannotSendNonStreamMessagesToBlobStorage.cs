using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotSendNonStreamMessagesToBlobStorageException : Exception
    {
        public CannotSendNonStreamMessagesToBlobStorageException() { }
        public CannotSendNonStreamMessagesToBlobStorageException(string message) : base(message) { }
        public CannotSendNonStreamMessagesToBlobStorageException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendNonStreamMessagesToBlobStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
