using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.File.IO.Exceptions
{

    [Serializable]
    public class CannotDeleteFileAsItDoesNotResideUnderFolderException : Exception
    {
        public CannotDeleteFileAsItDoesNotResideUnderFolderException() { }
        public CannotDeleteFileAsItDoesNotResideUnderFolderException(string message) : base(message) { }
        public CannotDeleteFileAsItDoesNotResideUnderFolderException(string message, Exception inner) : base(message, inner) { }
        protected CannotDeleteFileAsItDoesNotResideUnderFolderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
