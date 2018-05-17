using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotSendNonStringMessagesToBlobStorageException : Exception
    {
        public CannotSendNonStringMessagesToBlobStorageException() { }
        public CannotSendNonStringMessagesToBlobStorageException(string message) : base(message) { }
        public CannotSendNonStringMessagesToBlobStorageException(string message, Exception inner) : base(message, inner) { }
        protected CannotSendNonStringMessagesToBlobStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
