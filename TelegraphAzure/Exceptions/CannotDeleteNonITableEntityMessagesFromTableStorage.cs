using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotDeleteNonITableEntityMessagesFromTableStorageException : Exception
    {
        public CannotDeleteNonITableEntityMessagesFromTableStorageException() { }
        public CannotDeleteNonITableEntityMessagesFromTableStorageException(string message) : base(message) { }
        public CannotDeleteNonITableEntityMessagesFromTableStorageException(string message, Exception inner) : base(message, inner) { }
        protected CannotDeleteNonITableEntityMessagesFromTableStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
