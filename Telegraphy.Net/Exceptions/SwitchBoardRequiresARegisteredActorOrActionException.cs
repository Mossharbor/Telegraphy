using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class SwitchBoardRequiresARegisteredActorOrActionException : Exception
    {
        public SwitchBoardRequiresARegisteredActorOrActionException() { }
        public SwitchBoardRequiresARegisteredActorOrActionException(string message) : base(message) { }
        public SwitchBoardRequiresARegisteredActorOrActionException(string message, Exception inner) : base(message, inner) { }
        protected SwitchBoardRequiresARegisteredActorOrActionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
