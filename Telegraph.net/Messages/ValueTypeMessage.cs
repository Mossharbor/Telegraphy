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
        bool isArray = false;
        ulong arrayLength = 0;
        public ValueTypeMessage(T message)
        {
            this.Message = message;
        }

        public ValueTypeMessage(T[] message)
        {
            isArray = true;
            this.Message = message;
        }

        public ValueTypeMessage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            Type messageType = null;

            this.isArray = info.GetBoolean("isarray");
            this.arrayLength = info.GetUInt64("arraylength");
            string msgTypeString = info.GetString("msg_type");
            switch(msgTypeString.ToLower())
            {
                case "int32":
                    messageType = !isArray ? typeof(int) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "int32[]":
                    messageType = isArray ? typeof(int[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "uint32":
                    messageType = !isArray ? typeof(uint) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "uint32[]":
                    messageType = isArray ? typeof(uint[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "double":
                    messageType = !isArray ? typeof(double) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "double[]":
                    messageType = isArray ? typeof(double[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "single":
                    messageType = !isArray ? typeof(float) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "single[]":
                    messageType = isArray ? typeof(float[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "byte":
                    messageType = !isArray ? typeof(byte) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "byte[]":
                    messageType = isArray ? typeof(byte[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "sbyte":
                    messageType = !isArray ? typeof(sbyte) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "sbyte[]":
                    messageType = isArray ? typeof(sbyte[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "char":
                    messageType = !isArray ? typeof(char) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "char[]":
                    messageType = isArray ? typeof(char[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "int16":
                    messageType = !isArray ? typeof(short) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "int16[]":
                    messageType = isArray ? typeof(short[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "uint16":
                    messageType = !isArray ? typeof(ushort) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "uint16[]":
                    messageType = isArray ? typeof(ushort[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "int64":
                    messageType = !isArray ? typeof(long) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "int64[]":
                    messageType = isArray ? typeof(long[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "uint64":
                    messageType = !isArray ? typeof(ulong) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "uint64[]":
                    messageType = isArray ? typeof(ulong[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "boolean":
                    messageType = !isArray ? typeof(bool) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "boolean[]":
                    messageType = isArray ? typeof(bool[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "decimal":
                    messageType = !isArray ? typeof(decimal) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "decimal[]":
                    messageType = isArray ? typeof(decimal[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "datetime":
                    messageType = !isArray ? typeof(DateTime) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "datetime[]":
                    messageType = isArray ? typeof(DateTime[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "timespan":
                    messageType = !isArray ? typeof(TimeSpan) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "timespan[]":
                    messageType = isArray ? typeof(TimeSpan[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "guid":
                    messageType = !isArray ? typeof(Guid) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "guid[]":
                    messageType = isArray ? typeof(Guid[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "datetimeoffset":
                    messageType = !isArray ? typeof(DateTimeOffset) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "datetimeoffset[]":
                    messageType = isArray ? typeof(DateTimeOffset[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
                case "transitiontime":
                    messageType = !isArray ? typeof(TimeZoneInfo.TransitionTime) : throw new CannotSerializeArrayValueTypeWhenNonArraySpecifiedException(msgTypeString); break;
                case "transitiontime[]":
                    messageType = isArray ? typeof(TimeZoneInfo.TransitionTime[]) : throw new CannotSerializeNonArrayValueTypeWhenArraySpecifiedException(msgTypeString); break;
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
            info.AddValue("isarray", this.isArray, typeof(bool));
            info.AddValue("arraylength", this.arrayLength, typeof(ulong));
        }
        #endregion
    }
}
