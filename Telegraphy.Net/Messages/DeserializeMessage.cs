using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public class DeserializeMessage<MsgType> : IActorMessage, IActorMessageIdentifier
    {
        Type thisType = null;
        byte[] msgBytes;

        public DeserializeMessage(byte[] msgBytes)
        {
            this.Message = msgBytes;
            this.thisType = typeof(DeserializeMessage<MsgType>);
        }
        #region IActorMessageIdentifier
        private string id = null;
        public string Id
        {
            get { if (null == id) id = Guid.NewGuid().ToString(); return id; }
        }
        #endregion

        internal Type Type
        {
            get { return thisType; }
            set { thisType = value; }
        }

        public MsgType DeserializedMessage { get; set; }

        public object Message { get; set; }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public new Type GetType()
        {
            return Type;
        }
    }
}
