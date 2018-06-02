using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class MessageTypeHasAlreadyBeenRegisteredException : Exception
    {
        public MessageTypeHasAlreadyBeenRegisteredException() { }
        public MessageTypeHasAlreadyBeenRegisteredException(string message) : base(message) { }
        public MessageTypeHasAlreadyBeenRegisteredException(string message, Exception inner) : base(message, inner) { }
        protected MessageTypeHasAlreadyBeenRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
