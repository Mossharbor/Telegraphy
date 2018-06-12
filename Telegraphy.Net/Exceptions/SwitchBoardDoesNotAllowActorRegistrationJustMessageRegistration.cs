using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException : Exception
    {
        public SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException() { }
        public SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException(string message) : base(message) { }
        public SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException(string message, Exception inner) : base(message, inner) { }
        protected SwitchBoardDoesNotAllowActorRegistrationJustMessageRegistrationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
