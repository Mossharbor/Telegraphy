using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterOperatorException : FailedRegistrationException
    {
        public FailedToRegisterOperatorException() { }
        public FailedToRegisterOperatorException(string message) : base(message) { }
        public FailedToRegisterOperatorException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterOperatorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
