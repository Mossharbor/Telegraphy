using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterExpressionForTypeException : FailedRegistrationException
    {
        public FailedToRegisterExpressionForTypeException() { }
        public FailedToRegisterExpressionForTypeException(string message) : base(message) { }
        public FailedToRegisterExpressionForTypeException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterExpressionForTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
