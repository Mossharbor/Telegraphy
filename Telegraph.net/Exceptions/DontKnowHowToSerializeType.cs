using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{

    [Serializable]
    public class DontKnowHowToSerializeTypeException : Exception
    {
        public DontKnowHowToSerializeTypeException() { }
        public DontKnowHowToSerializeTypeException(string message) : base(message) { }
        public DontKnowHowToSerializeTypeException(string message, Exception inner) : base(message, inner) { }
        protected DontKnowHowToSerializeTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
