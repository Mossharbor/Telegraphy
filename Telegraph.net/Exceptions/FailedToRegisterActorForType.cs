using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterActorForTypeException : FailedRegistrationException
    {
        public FailedToRegisterActorForTypeException() { }
        public FailedToRegisterActorForTypeException(string message) : base(message) { }
        public FailedToRegisterActorForTypeException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterActorForTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
