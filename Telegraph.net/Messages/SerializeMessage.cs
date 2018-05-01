using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public class SerializeMessage : IActorMessage, IActorMessageIdentifier
    {
        Type thisType = null;
        IActorMessage wrappedMsg;
        public SerializeMessage(IActorMessage wrappedMsg) :this(wrappedMsg,null)
        {
        }

        internal SerializeMessage(IActorMessage wrappedMsg, object result)
        {
            this.wrappedMsg = wrappedMsg;
            //this.thisType = wrappedMsg.GetType();
            this.Status = null;
            this.ProcessingResult = result;
        }
        #region IActorMessageIdentifier
        private string id = null;
        public string Id
        {
            get { if (null == id) id = Guid.NewGuid().ToString(); return id; }
        }
        #endregion

        internal IActorMessage MessageToSerialize { get { return wrappedMsg; } }

        public object Message { get { return this.wrappedMsg.Message; } set { throw new NotImplementedException("Cannot set the Message for the SerializedMessage class."); } }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }

        //public new Type GetType()
        //{
        //    if (null == thisType)
        //        thisType = this.Message.GetType();
        //    return thisType;
        //}
    }
}
