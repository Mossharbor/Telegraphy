using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.File.IO.Exceptions
{

    [Serializable]
    public class CannotDeleteFilesInFolderFileNameIsNotAStringException : Exception
    {
        public CannotDeleteFilesInFolderFileNameIsNotAStringException() { }
        public CannotDeleteFilesInFolderFileNameIsNotAStringException(string message) : base(message) { }
        public CannotDeleteFilesInFolderFileNameIsNotAStringException(string message, Exception inner) : base(message, inner) { }
        protected CannotDeleteFilesInFolderFileNameIsNotAStringException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
