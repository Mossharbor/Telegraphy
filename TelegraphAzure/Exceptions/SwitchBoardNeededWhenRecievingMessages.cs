using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class SwitchBoardNeededWhenRecievingMessagesException : NullReferenceException
    {
        public SwitchBoardNeededWhenRecievingMessagesException() { }
        public SwitchBoardNeededWhenRecievingMessagesException(string message) : base(message) { }
        public SwitchBoardNeededWhenRecievingMessagesException(string message, Exception inner) : base(message, inner) { }
        protected SwitchBoardNeededWhenRecievingMessagesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
