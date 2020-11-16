using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotSentNonITableEntityMessagesToTableStorageException : Exception
    {
        public CannotSentNonITableEntityMessagesToTableStorageException() { }
        public CannotSentNonITableEntityMessagesToTableStorageException(string message) : base(message) { }
        public CannotSentNonITableEntityMessagesToTableStorageException(string message, Exception inner) : base(message, inner) { }
        protected CannotSentNonITableEntityMessagesToTableStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
