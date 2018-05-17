using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{
    [Serializable]
    public class CannotSendNonByteArrayMessagesToBlobStorageException : Exception
    {
        public CannotSendNonByteArrayMessagesToBlobStorageException() { }
        public CannotSendNonByteArrayMessagesToBlobStorageException(string message) : base(message) { }
        public CannotSendNonByteArrayMessagesToBlobStorageException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendNonByteArrayMessagesToBlobStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
