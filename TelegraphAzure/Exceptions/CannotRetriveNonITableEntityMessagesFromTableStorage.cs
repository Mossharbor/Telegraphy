using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class CannotRetriveNonITableEntityMessagesFromTableStorageException : Exception
    {
        public CannotRetriveNonITableEntityMessagesFromTableStorageException() { }
        public CannotRetriveNonITableEntityMessagesFromTableStorageException(string message) : base(message) { }
        public CannotRetriveNonITableEntityMessagesFromTableStorageException(string message, Exception inner) : base(message, inner) { }
        protected CannotRetriveNonITableEntityMessagesFromTableStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
