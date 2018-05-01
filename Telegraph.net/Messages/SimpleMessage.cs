using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Runtime.Serialization;
    
    public class SimpleMessage<T> : IActorMessage  where T : class
    {
        public SimpleMessage()
        {
            this.thisType = typeof(T);
            this.Status = null;
        }

        protected SimpleMessage(SerializationInfo info, StreamingContext context, Type messageType)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.Message = info.GetValue("msg", messageType);
            this.ThisType = messageType;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("msg", this.Message, this.Message.GetType());
        }

        public SimpleMessage(T message)
        {
            this.thisType = typeof(T);
            if (message is IActorMessage)
            {
                this.OriginalMessage = message;
                this.Message = ((IActorMessage)message).Message;
                this.Status = ((IActorMessage)message).Status;
                this.thisType = message.GetType();
            }
            else
                this.Message = message;
        }

        private Type thisType = null;
        protected Type ThisType
        {
            get { return thisType; }
            set { thisType = value; }
        }

        public T OriginalMessage { get; set; }

        public object Message { get; set; }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public new Type GetType()
        {
            if (null == thisType)
                thisType = this.Message.GetType();
            return thisType;
        }
    }
}
