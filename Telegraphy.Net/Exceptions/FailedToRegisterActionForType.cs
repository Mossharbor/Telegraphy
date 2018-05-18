using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterActionForTypeException : FailedRegistrationException
    {
        public FailedToRegisterActionForTypeException() { }
        public FailedToRegisterActionForTypeException(string message) : base(message) { }
        public FailedToRegisterActionForTypeException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterActionForTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
