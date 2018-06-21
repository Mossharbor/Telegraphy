using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public class SerializeMessage<MsgType> : IActorMessage, IActorMessageIdentifier
    {
        Type thisType = null;
        MsgType wrappedMsg;
        public SerializeMessage(MsgType wrappedMsg) :this(wrappedMsg,null)
        {
        }

        internal SerializeMessage(MsgType wrappedMsg, object result)
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

        internal MsgType MessageToSerialize { get { return wrappedMsg; } }

        public object Message
        {
            get
            {
                if (this.wrappedMsg is IActorMessage)
                    return (this.wrappedMsg as IActorMessage).Message;
                return this.wrappedMsg;
            }
            set { throw new NotImplementedException("Cannot set the Message for the SerializedMessage class."); } }

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
