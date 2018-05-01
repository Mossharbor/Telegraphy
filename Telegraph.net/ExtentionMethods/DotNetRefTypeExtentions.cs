using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public static class DotNetRefTypeExtentions
    {
        public static IActorMessage ToActorMessage(this int self)
        {
            return new ValueTypeMessage<int>(self);
        }

        public static IActorMessage ToActorMessage(this uint self)
        {
            return new ValueTypeMessage<uint>(self);
        }

        public static IActorMessage ToActorMessage(this float self)
        {
            return new ValueTypeMessage<float>(self);
        }

        public static IActorMessage ToActorMessage(this double self)
        {
            return new ValueTypeMessage<double>(self);
        }

        public static IActorMessage ToActorMessage(this char self)
        {
            return new ValueTypeMessage<char>(self);
        }

        public static IActorMessage ToActorMessage(this decimal self)
        {
            return new ValueTypeMessage<decimal>(self);
        }

        public static IActorMessage ToActorMessage(this bool self)
        {
            return new ValueTypeMessage<bool>(self);
        }

        public static IActorMessage ToActorMessage(this DateTime self)
        {
            return new ValueTypeMessage<DateTime>(self);
        }

        public static IActorMessage ToActorMessage(this byte self)
        {
            return new ValueTypeMessage<byte>(self);
        }

        public static IActorMessage ToActorMessage(this ushort self)
        {
            return new ValueTypeMessage<ushort>(self);
        }

        public static IActorMessage ToActorMessage(this short self)
        {
            return new ValueTypeMessage<short>(self);
        }

        public static IActorMessage ToActorMessage(this ulong self)
        {
            return new ValueTypeMessage<ulong>(self);
        }

        public static IActorMessage ToActorMessage(this long self)
        {
            return new ValueTypeMessage<long>(self);
        }

        public static IActorMessage ToActorMessage(this sbyte self)
        {
            return new ValueTypeMessage<sbyte>(self);
        }
    }
}
