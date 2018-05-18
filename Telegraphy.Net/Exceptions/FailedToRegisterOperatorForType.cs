using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterOperatorForTypeException : FailedToRegisterOperatorException
    {
        public FailedToRegisterOperatorForTypeException() { }
        public FailedToRegisterOperatorForTypeException(string message) : base(message) { }
        public FailedToRegisterOperatorForTypeException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterOperatorForTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
