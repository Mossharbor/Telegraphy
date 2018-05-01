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
    public class Ping : IActorMessage, ISerializable
    {
        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public Ping(string payload) 
        {
            this.Message = payload;
        }

        public Ping() : this("Ping") { }

        protected Ping(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.Message = info.GetString("msg");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("msg", this.Message);
        }
    }
}
