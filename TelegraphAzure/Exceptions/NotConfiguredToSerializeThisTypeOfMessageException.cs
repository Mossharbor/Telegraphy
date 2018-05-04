﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure.Exceptions
{

    [Serializable]
    public class NotConfiguredToSerializeThisTypeOfMessageException : Exception
    {
        public NotConfiguredToSerializeThisTypeOfMessageException() { }
        public NotConfiguredToSerializeThisTypeOfMessageException(string message) : base(message) { }
        public NotConfiguredToSerializeThisTypeOfMessageException(string message, Exception inner) : base(message, inner) { }
        protected NotConfiguredToSerializeThisTypeOfMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
