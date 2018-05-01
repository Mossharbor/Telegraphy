using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPong
{
    using System.Runtime.Serialization;
    using Telegraphy.Net;

    [Serializable]
    public class Pong : SimpleMessage<Pong>, ISerializable
    {
        public Pong()
        {
            this.Message = "Pong";
            this.ThisType = typeof(Pong);
        }

        public Pong(SerializationInfo info, StreamingContext context) : base (info, context, typeof(string))
        {
            this.ThisType = typeof(Pong);
        }
    }
}
