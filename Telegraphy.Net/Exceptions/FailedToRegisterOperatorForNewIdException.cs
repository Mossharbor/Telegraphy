using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterOperatorForNewIdException : FailedToRegisterOperatorException
    {
        public FailedToRegisterOperatorForNewIdException() { }
        public FailedToRegisterOperatorForNewIdException(string message) : base(message) { }
        public FailedToRegisterOperatorForNewIdException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterOperatorForNewIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
