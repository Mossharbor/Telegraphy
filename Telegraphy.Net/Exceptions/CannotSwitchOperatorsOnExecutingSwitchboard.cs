using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    [Serializable]
    public class CannotSwitchOperatorsOnExecutingSwitchboardException : InvalidOperationException
    {
        public CannotSwitchOperatorsOnExecutingSwitchboardException() { }
        public CannotSwitchOperatorsOnExecutingSwitchboardException(string message) : base(message) { }
        public CannotSwitchOperatorsOnExecutingSwitchboardException(string message, Exception inner) : base(message, inner) { }
        protected CannotSwitchOperatorsOnExecutingSwitchboardException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
