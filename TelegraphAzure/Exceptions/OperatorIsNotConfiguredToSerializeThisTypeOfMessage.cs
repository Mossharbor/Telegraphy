using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class OperatorIsNotConfiguredToSerializeThisTypeOfMessageException : Exception
    {
        public OperatorIsNotConfiguredToSerializeThisTypeOfMessageException() { }
        public OperatorIsNotConfiguredToSerializeThisTypeOfMessageException(string message) : base(message) { }
        public OperatorIsNotConfiguredToSerializeThisTypeOfMessageException(string message, Exception inner) : base(message, inner) { }
        protected OperatorIsNotConfiguredToSerializeThisTypeOfMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
