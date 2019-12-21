using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public static class DotNetValueTypeExtensions
    {
        public static IActorMessage ToActorMessage(this string self)
        {
            // NOTE this is not a value type but it is so common we added here.
            return new SimpleMessage<string>(self);
        }

        public static IActorMessage ToActorMessage(this int self)
        {
            return new ValueTypeMessage<int>(self);
        }

        public static IActorMessage ToActorMessage(this int[] self)
        {
            return new ValueArrayTypeMessage<int>(self);
        }

        public static IActorMessage ToActorMessage(this uint self)
        {
            return new ValueTypeMessage<uint>(self);
        }

        public static IActorMessage ToActorMessage(this uint[] self)
        {
            return new ValueArrayTypeMessage<uint>(self);
        }

        public static IActorMessage ToActorMessage(this float self)
        {
            return new ValueTypeMessage<float>(self);
        }

        public static IActorMessage ToActorMessage(this float[] self)
        {
            return new ValueArrayTypeMessage<float>(self);
        }

        public static IActorMessage ToActorMessage(this double self)
        {
            return new ValueTypeMessage<double>(self);
        }

        public static IActorMessage ToActorMessage(this double[] self)
        {
            return new ValueArrayTypeMessage<double>(self);
        }

        public static IActorMessage ToActorMessage(this char self)
        {
            return new ValueTypeMessage<char>(self);
        }

        public static IActorMessage ToActorMessage(this char[] self)
        {
            return new ValueArrayTypeMessage<char>(self);
        }

        public static IActorMessage ToActorMessage(this decimal self)
        {
            return new ValueTypeMessage<decimal>(self);
        }

        public static IActorMessage ToActorMessage(this decimal[] self)
        {
            return new ValueArrayTypeMessage<decimal>(self);
        }

        public static IActorMessage ToActorMessage(this bool self)
        {
            return new ValueTypeMessage<bool>(self);
        }

        public static IActorMessage ToActorMessage(this bool[] self)
        {
            return new ValueArrayTypeMessage<bool>(self);
        }

        public static IActorMessage ToActorMessage(this DateTime self)
        {
            return new ValueTypeMessage<DateTime>(self);
        }

        public static IActorMessage ToActorMessage(this DateTime[] self)
        {
            return new ValueArrayTypeMessage<DateTime>(self);
        }

        public static IActorMessage ToActorMessage(this byte self)
        {
            return new ValueTypeMessage<byte>(self);
        }

        public static IActorMessage ToActorMessage(this byte[] self)
        {
            return new ValueArrayTypeMessage<byte>(self);
        }

        public static IActorMessage ToActorMessage(this ushort self)
        {
            return new ValueTypeMessage<ushort>(self);
        }

        public static IActorMessage ToActorMessage(this ushort[] self)
        {
            return new ValueArrayTypeMessage<ushort>(self);
        }

        public static IActorMessage ToActorMessage(this short self)
        {
            return new ValueTypeMessage<short>(self);
        }

        public static IActorMessage ToActorMessage(this short[] self)
        {
            return new ValueArrayTypeMessage<short>(self);
        }

        public static IActorMessage ToActorMessage(this ulong self)
        {
            return new ValueTypeMessage<ulong>(self);
        }

        public static IActorMessage ToActorMessage(this ulong[] self)
        {
            return new ValueArrayTypeMessage<ulong>(self);
        }

        public static IActorMessage ToActorMessage(this long self)
        {
            return new ValueTypeMessage<long>(self);
        }

        public static IActorMessage ToActorMessage(this long[] self)
        {
            return new ValueArrayTypeMessage<long>(self);
        }

        public static IActorMessage ToActorMessage(this sbyte self)
        {
            return new ValueTypeMessage<sbyte>(self);
        }

        public static IActorMessage ToActorMessage(this sbyte[] self)
        {
            return new ValueArrayTypeMessage<sbyte>(self);
        }

        public static IActorMessage ToActorMessage(this Guid self)
        {
            return new ValueTypeMessage<Guid>(self);
        }

        public static IActorMessage ToActorMessage(this Guid[] self)
        {
            return new ValueArrayTypeMessage<Guid>(self);
        }

        public static IActorMessage ToActorMessage(this TimeSpan self)
        {
            return new ValueTypeMessage<TimeSpan>(self);
        }

        public static IActorMessage ToActorMessage(this TimeSpan[] self)
        {
            return new ValueArrayTypeMessage<TimeSpan>(self);
        }
    }
}
