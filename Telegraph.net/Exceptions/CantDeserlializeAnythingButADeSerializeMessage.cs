using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class CantDeserlializeAnythingButADeSerializeMessageException : Exception
    {
        public CantDeserlializeAnythingButADeSerializeMessageException() { }
        public CantDeserlializeAnythingButADeSerializeMessageException(string message) : base(message) { }
        public CantDeserlializeAnythingButADeSerializeMessageException(string message, Exception inner) : base(message, inner) { }
        protected CantDeserlializeAnythingButADeSerializeMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
