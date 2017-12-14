﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    [Serializable]
    public class NoActorForMessageTypeException : Exception
    {
        public NoActorForMessageTypeException() { }
        public NoActorForMessageTypeException(string message) : base(message) { }
        public NoActorForMessageTypeException(string message, Exception inner) : base(message, inner) { }
        protected NoActorForMessageTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
