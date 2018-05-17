using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net.Exceptions
{

    [Serializable]
    public class CantSendFileDataWhenFileDoesNotExistException : Exception
    {
        public CantSendFileDataWhenFileDoesNotExistException() { }
        public CantSendFileDataWhenFileDoesNotExistException(string message) : base(message) { }
        public CantSendFileDataWhenFileDoesNotExistException(string message, Exception inner) : base(message, inner) { }
        protected CantSendFileDataWhenFileDoesNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
