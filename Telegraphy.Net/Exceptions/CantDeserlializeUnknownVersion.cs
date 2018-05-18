using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class CantDeserlializeUnknownVersionException : Exception
    {
        public CantDeserlializeUnknownVersionException() { }
        public CantDeserlializeUnknownVersionException(string message) : base(message) { }
        public CantDeserlializeUnknownVersionException(string message, Exception inner) : base(message, inner) { }
        protected CantDeserlializeUnknownVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
