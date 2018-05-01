using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public class ValueTypeMessage <T> : IActorMessage where T : IEquatable<T>
    {
        public ValueTypeMessage(T message)
        {
            this.Message = message;
        }
        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }
    }
}
