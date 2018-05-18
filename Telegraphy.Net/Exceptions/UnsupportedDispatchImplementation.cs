using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class UnsupportedDispatchImplementationException : Exception
    {
        public UnsupportedDispatchImplementationException() { }
        public UnsupportedDispatchImplementationException(string message) : base(message) { }
        public UnsupportedDispatchImplementationException(string message, Exception inner) : base(message, inner) { }
        protected UnsupportedDispatchImplementationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
