using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.IO.Exceptions
{

    [Serializable]
    public class CouldNotAcquireLockOnQueueInTimeAllottedException : Exception
    {
        public CouldNotAcquireLockOnQueueInTimeAllottedException() { }
        public CouldNotAcquireLockOnQueueInTimeAllottedException(string message) : base(message) { }
        public CouldNotAcquireLockOnQueueInTimeAllottedException(string message, Exception inner) : base(message, inner) { }
        protected CouldNotAcquireLockOnQueueInTimeAllottedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
