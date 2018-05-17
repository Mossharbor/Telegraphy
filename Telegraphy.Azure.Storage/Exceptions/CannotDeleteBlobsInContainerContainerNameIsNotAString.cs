using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotDeleteBlobsInContainerContainerNameIsNotAStringException : Exception
    {
        public CannotDeleteBlobsInContainerContainerNameIsNotAStringException() { }
        public CannotDeleteBlobsInContainerContainerNameIsNotAStringException(string message) : base(message) { }
        public CannotDeleteBlobsInContainerContainerNameIsNotAStringException(string message, Exception inner) : base(message, inner) { }
        protected CannotDeleteBlobsInContainerContainerNameIsNotAStringException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
