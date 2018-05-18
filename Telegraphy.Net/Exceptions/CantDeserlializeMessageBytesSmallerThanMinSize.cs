using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class CantDeserlializeMessageBytesSmallerThanMinSizeException : Exception
    {
        public CantDeserlializeMessageBytesSmallerThanMinSizeException() { }
        public CantDeserlializeMessageBytesSmallerThanMinSizeException(string message) : base(message) { }
        public CantDeserlializeMessageBytesSmallerThanMinSizeException(string message, Exception inner) : base(message, inner) { }
        protected CantDeserlializeMessageBytesSmallerThanMinSizeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
