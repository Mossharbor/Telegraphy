using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class FailedToRegisterActorInvocationForTypeDeserializationException : FailedRegistrationException
    {
        public FailedToRegisterActorInvocationForTypeDeserializationException() { }
        public FailedToRegisterActorInvocationForTypeDeserializationException(string message) : base(message) { }
        public FailedToRegisterActorInvocationForTypeDeserializationException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterActorInvocationForTypeDeserializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
