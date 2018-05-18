using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterActorInvocationForActionException : FailedRegistrationException
    {
        public FailedToRegisterActorInvocationForActionException() { }
        public FailedToRegisterActorInvocationForActionException(string message) : base(message) { }
        public FailedToRegisterActorInvocationForActionException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterActorInvocationForActionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
