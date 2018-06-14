using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Relay.Hybrid
{

    [Serializable]
    public class HybridConnectionNameNoSpecifiedInEntityPathException : Exception
    {
        public HybridConnectionNameNoSpecifiedInEntityPathException() { }
        public HybridConnectionNameNoSpecifiedInEntityPathException(string message) : base(message) { }
        public HybridConnectionNameNoSpecifiedInEntityPathException(string message, Exception inner) : base(message, inner) { }
        protected HybridConnectionNameNoSpecifiedInEntityPathException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
