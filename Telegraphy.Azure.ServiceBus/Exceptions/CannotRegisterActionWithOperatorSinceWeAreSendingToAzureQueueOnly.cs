using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException : Exception
    {
        public CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException() { }
        public CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException(string message) : base(message) { }
        public CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException(string message, Exception inner) : base(message, inner) { }
        protected CannotRegisterActionWithOperatorSinceWeAreSendingToAzureQueueOnlyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
