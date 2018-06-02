using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class OperatorHasNoSwitchBoardException : OperatorCannotSendMessagesException
    {
        public OperatorHasNoSwitchBoardException() { }
        public OperatorHasNoSwitchBoardException(string message) : base(message) { }
        public OperatorHasNoSwitchBoardException(string message, Exception inner) : base(message, inner) { }
        protected OperatorHasNoSwitchBoardException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
