using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{

    [Serializable]
    public class CannotSerializeNonArrayValueTypeWhenArraySpecifiedException : Exception
    {
        public CannotSerializeNonArrayValueTypeWhenArraySpecifiedException() { }
        public CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(string message) : base(message) { }
        public CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(string message, Exception inner) : base(message, inner) { }
        protected CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
