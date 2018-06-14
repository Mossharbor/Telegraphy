using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException : Exception
    {
        public CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException() { }
        public CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException(string message) : base(message) { }
        public CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException(string message, Exception inner) : base(message, inner) { }
        protected CannotRegisterExpressionSinceOperatorHasNoSwitchBoardsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
