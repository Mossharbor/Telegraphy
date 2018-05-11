using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class MessageTypeWasNotAFileNameCannotDownloadBlobDataException : Exception
    {
        public MessageTypeWasNotAFileNameCannotDownloadBlobDataException() { }
        public MessageTypeWasNotAFileNameCannotDownloadBlobDataException(string message) : base(message) { }
        public MessageTypeWasNotAFileNameCannotDownloadBlobDataException(string message, Exception inner) : base(message, inner) { }
        protected MessageTypeWasNotAFileNameCannotDownloadBlobDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
