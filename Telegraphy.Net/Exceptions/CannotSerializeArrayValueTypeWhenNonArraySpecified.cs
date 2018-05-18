using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{

    [Serializable]
    public class CannotSerializeArrayValueTypeWhenNonArraySpecifiedException : Exception
    {
        public CannotSerializeArrayValueTypeWhenNonArraySpecifiedException() { }
        public CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(string message) : base(message) { }
        public CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(string message, Exception inner) : base(message, inner) { }
        protected CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
