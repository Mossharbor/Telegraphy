
namespace Telegraphy.IO.Exceptions
{
    [System.Serializable]
    public class CannotDeleteFilesInFolderNameIsNotAStringException : System.Exception
    {
        public CannotDeleteFilesInFolderNameIsNotAStringException() { }
        public CannotDeleteFilesInFolderNameIsNotAStringException(string message) : base(message) { }
        public CannotDeleteFilesInFolderNameIsNotAStringException(string message, System.Exception inner) : base(message, inner) { }
        protected CannotDeleteFilesInFolderNameIsNotAStringException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}