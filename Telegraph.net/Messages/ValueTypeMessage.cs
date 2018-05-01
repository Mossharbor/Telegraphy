using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Runtime.Serialization;

    [Serializable]
    public class ValueTypeMessage<T> : IActorMessage, IActorMessageIdentifier, ISerializable where T : IEquatable<T>
    {
        public ValueTypeMessage(T message)
        {
            this.Message = message;
        }

        public ValueTypeMessage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            Type messageType = null;

            string msgTypeString = info.GetString("msg_type");
            switch(msgTypeString.ToLower())
            {
                case "int32":
                    messageType = typeof(int); break;
                case "uint32":
                    messageType = typeof(uint); break;
                case "double":
                    messageType = typeof(double); break;
                case "single":
                    messageType = typeof(float); break;
                case "byte":
                    messageType = typeof(byte); break;
                case "sbyte":
                    messageType = typeof(sbyte); break;
                case "char":
                    messageType = typeof(char); break;
                case "int16":
                    messageType = typeof(short); break;
                case "uint16":
                    messageType = typeof(ushort); break;
                case "int64":
                    messageType = typeof(long); break;
                case "uint64":
                    messageType = typeof(ulong); break;
                case "boolean":
                    messageType = typeof(bool); break;
                case "decimal":
                    messageType = typeof(decimal); break;
                case "datetime":
                    messageType = typeof(DateTime); break;
                case "timespan":
                    messageType = typeof(TimeSpan); break;
                default:
                    throw new DontKnowHowToSerializeTypeException(msgTypeString);
            }

            this.Message = info.GetValue("msg", messageType);
            this.id = info.GetString("id");
        }

        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }

        #region IActorMessageIdentifier
        private string id = null;
        public string Id
        {
            get { if (null == id) id = Guid.NewGuid().ToString(); return id; }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("msg_type", this.Message.GetType().Name, typeof(string));
            info.AddValue("msg", this.Message, this.Message.GetType());
            info.AddValue("id", this.Id, typeof(string));
        }
        #endregion
    }
}
