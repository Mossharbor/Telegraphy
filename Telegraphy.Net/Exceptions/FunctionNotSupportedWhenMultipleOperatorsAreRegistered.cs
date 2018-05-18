using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException : Exception
    {
        public FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException() { }
        public FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException(string message) : base(message) { }
        public FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException(string message, Exception inner) : base(message, inner) { }
        protected FunctionNotSupportedWhenMultipleOperatorsAreRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
