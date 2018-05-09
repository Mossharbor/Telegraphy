using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CantSendFileDataWhenFileDoesNotExistException : System.IO.FileNotFoundException
    {
        public CantSendFileDataWhenFileDoesNotExistException() { }
        public CantSendFileDataWhenFileDoesNotExistException(string message) : base(message) { }
        public CantSendFileDataWhenFileDoesNotExistException(string message, Exception inner) : base(message, inner) { }
        protected CantSendFileDataWhenFileDoesNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
