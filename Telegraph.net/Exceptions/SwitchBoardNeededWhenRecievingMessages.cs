using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net.Exceptions
{

    [Serializable]
    public class SwitchBoardNeededWhenRecievingMessagesException : Exception
    {
        public SwitchBoardNeededWhenRecievingMessagesException() { }
        public SwitchBoardNeededWhenRecievingMessagesException(string message) : base(message) { }
        public SwitchBoardNeededWhenRecievingMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SwitchBoardNeededWhenRecievingMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
