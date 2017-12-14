using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class NoOperatorFoundForIDException : Exception
    {
        public NoOperatorFoundForIDException() { }
        public NoOperatorFoundForIDException(string message) : base(message) { }
        public NoOperatorFoundForIDException(string message, Exception inner) : base(message, inner) { }
        protected NoOperatorFoundForIDException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
