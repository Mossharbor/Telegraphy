using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public class DeSerializeMessage : IActorMessage, IActorMessageIdentifier
    {
        Type thisType = null;
        byte[] msgBytes;

        public DeSerializeMessage(byte[] msgBytes)
        {
            this.Message = msgBytes;
            this.thisType = typeof(DeSerializeMessage);
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

        public object Message { get; set; }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public new Type GetType()
        {
            return Type;
        }
    }
}
