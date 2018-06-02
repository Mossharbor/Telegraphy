using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{

    [Serializable]
    public class ActorHasAlreadyBeenRegisteredException : Exception
    {
        public ActorHasAlreadyBeenRegisteredException() { }
        public ActorHasAlreadyBeenRegisteredException(string message) : base(message) { }
        public ActorHasAlreadyBeenRegisteredException(string message, Exception inner) : base(message, inner) { }
        protected ActorHasAlreadyBeenRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
