using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FailedRegistrationException : Exception
    {
        public FailedRegistrationException() { }
        public FailedRegistrationException(string message) : base(message) { }
        public FailedRegistrationException(string message, Exception inner) : base(message, inner) { }
        protected FailedRegistrationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
