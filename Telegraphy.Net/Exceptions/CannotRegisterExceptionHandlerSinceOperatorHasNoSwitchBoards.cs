using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException : Exception
    {
        public CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException() { }
        public CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException(string message) : base(message) { }
        public CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException(string message, Exception inner) : base(message, inner) { }
        protected CannotRegisterExceptionHandlerSinceOperatorHasNoSwitchBoardsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
