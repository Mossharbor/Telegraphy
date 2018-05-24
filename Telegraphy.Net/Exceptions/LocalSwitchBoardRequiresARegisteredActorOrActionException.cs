using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net.Exceptions
{

    [Serializable]
    public class LocalSwitchBoardRequiresARegisteredActorOrActionException : Exception
    {
        public LocalSwitchBoardRequiresARegisteredActorOrActionException() { }
        public LocalSwitchBoardRequiresARegisteredActorOrActionException(string message) : base(message) { }
        public LocalSwitchBoardRequiresARegisteredActorOrActionException(string message, Exception inner) : base(message, inner) { }
        protected LocalSwitchBoardRequiresARegisteredActorOrActionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
