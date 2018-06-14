using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class CannotRegisterActionSinceOperatorHasNoSwitchBoardsException : Exception
    {
        public CannotRegisterActionSinceOperatorHasNoSwitchBoardsException() { }
        public CannotRegisterActionSinceOperatorHasNoSwitchBoardsException(string message) : base(message) { }
        public CannotRegisterActionSinceOperatorHasNoSwitchBoardsException(string message, Exception inner) : base(message, inner) { }
        protected CannotRegisterActionSinceOperatorHasNoSwitchBoardsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
