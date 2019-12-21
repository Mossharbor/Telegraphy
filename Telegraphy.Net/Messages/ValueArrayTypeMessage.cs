using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    using System.Runtime.Serialization;

    [Serializable]
    public class ValueArrayTypeMessage<T> : ValueTypeMessage<T> where T : IEquatable<T>
    {
        public ValueArrayTypeMessage(T[] message): base (message)
        {
        }

        public ValueArrayTypeMessage(SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
        }

        internal static Type GetArrayType(Type type)
        {
            string msgTypeString = type.Name;
            switch (msgTypeString.ToLower())
            {
                case "int32":
                    return typeof(int[]);
                case "uint32":
                    return typeof(uint[]);
                case "double":
                    return typeof(double[]);
                case "single":
                    return typeof(float[]);
                case "byte":
                    return typeof(byte[]);
                case "sbyte":
                    return typeof(sbyte[]);
                case "char":
                    return typeof(char[]);
                case "int16":
                    return typeof(short[]);
                case "uint16":
                    return typeof(ushort[]);
                case "int64":
                    return typeof(long[]);
                case "uint64":
                    return typeof(ulong[]);
                case "boolean":
                    return typeof(bool[]);
                case "decimal":
                    return typeof(decimal[]);
                case "datetime":
                    return typeof(DateTime[]);
                case "timespan":
                    return typeof(TimeSpan[]);
                case "guid":
                    return typeof(Guid[]);
                case "datetimeoffset":
                    return typeof(DateTimeOffset[]);
                case "transitiontime":
                    return typeof(TimeZoneInfo.TransitionTime[]);
                default:
                    throw new DontKnowHowToSerializeTypeException(msgTypeString);
            }
        }
    }
}
