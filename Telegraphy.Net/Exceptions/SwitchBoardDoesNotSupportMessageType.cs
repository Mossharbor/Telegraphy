using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class SwitchBoardDoesNotSupportMessageTypeException : Exception
    {
        public SwitchBoardDoesNotSupportMessageTypeException() { }
        public SwitchBoardDoesNotSupportMessageTypeException(string message) : base(message) { }
        public SwitchBoardDoesNotSupportMessageTypeException(string message, Exception inner) : base(message, inner) { }
        protected SwitchBoardDoesNotSupportMessageTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
