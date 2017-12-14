using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterActorInvocationForTypeException : FailedRegistrationException
    {
        public FailedToRegisterActorInvocationForTypeException() { }
        public FailedToRegisterActorInvocationForTypeException(string message) : base(message) { }
        public FailedToRegisterActorInvocationForTypeException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterActorInvocationForTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    
}
