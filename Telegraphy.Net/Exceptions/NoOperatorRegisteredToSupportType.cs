using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class NoOperatorRegisteredToSupportTypeException : Exception
    {
        public NoOperatorRegisteredToSupportTypeException() { }
        public NoOperatorRegisteredToSupportTypeException(string message) : base(message) { }
        public NoOperatorRegisteredToSupportTypeException(string message, Exception inner) : base(message, inner) { }
        protected NoOperatorRegisteredToSupportTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
