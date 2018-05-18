using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedToRegisterOperatorForActionException : FailedToRegisterOperatorException
    {
        public FailedToRegisterOperatorForActionException() { }
        public FailedToRegisterOperatorForActionException(string message) : base(message) { }
        public FailedToRegisterOperatorForActionException(string message, Exception inner) : base(message, inner) { }
        protected FailedToRegisterOperatorForActionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
