using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    public class FailedToRegisterOperatorForExistingIdException : FailedToRegisterOperatorException
    {
        public FailedToRegisterOperatorForExistingIdException() { }
        public FailedToRegisterOperatorForExistingIdException(string message) : base(message) { }
        public FailedToRegisterOperatorForExistingIdException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterOperatorForExistingIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
