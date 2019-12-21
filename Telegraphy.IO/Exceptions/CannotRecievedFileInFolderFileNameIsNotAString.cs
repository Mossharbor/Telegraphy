using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.IO.Exceptions
{

    [Serializable]
    public class CannotRecievedFileInFolderFileNameIsNotAStringException : Exception
    {
        public CannotRecievedFileInFolderFileNameIsNotAStringException() { }
        public CannotRecievedFileInFolderFileNameIsNotAStringException(string message) : base(message) { }
        public CannotRecievedFileInFolderFileNameIsNotAStringException(string message, Exception inner) : base(message, inner) { }
        protected CannotRecievedFileInFolderFileNameIsNotAStringException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
