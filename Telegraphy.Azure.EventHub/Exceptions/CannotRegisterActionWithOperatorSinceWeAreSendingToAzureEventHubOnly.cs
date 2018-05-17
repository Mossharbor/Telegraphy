using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotRegisterActionWithOperatorSinceWeAreSendingToAzureEventHubOnlyException : Exception
    {
        public CannotRegisterActionWithOperatorSinceWeAreSendingToAzureEventHubOnlyException() { }
        public CannotRegisterActionWithOperatorSinceWeAreSendingToAzureEventHubOnlyException(string message) : base(message) { }
        public CannotRegisterActionWithOperatorSinceWeAreSendingToAzureEventHubOnlyException(string message, Exception inner) : base(message, inner) { }
        protected CannotRegisterActionWithOperatorSinceWeAreSendingToAzureEventHubOnlyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
