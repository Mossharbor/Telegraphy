using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class DispatchMethodCanOnlyBeSetBeforeOperatorsAreRegisteredException : FailedRegistrationException
    {
        public DispatchMethodCanOnlyBeSetBeforeOperatorsAreRegisteredException() { }
        public DispatchMethodCanOnlyBeSetBeforeOperatorsAreRegisteredException(string message) : base(message) { }
        public DispatchMethodCanOnlyBeSetBeforeOperatorsAreRegisteredException(string message, Exception inner) : base(message, inner) { }
        protected DispatchMethodCanOnlyBeSetBeforeOperatorsAreRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
