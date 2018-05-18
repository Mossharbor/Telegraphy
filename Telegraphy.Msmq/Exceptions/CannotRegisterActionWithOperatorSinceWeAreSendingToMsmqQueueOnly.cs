using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Msmq.Exceptions
{

    [Serializable]
    public class CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException : Exception
    {
        public CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException() { }
        public CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException(string message) : base(message) { }
        public CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException(string message, Exception inner) : base(message, inner) { }
        protected CannotRegisterActionWithOperatorSinceWeAreSendingToMsmqQueueOnlyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
