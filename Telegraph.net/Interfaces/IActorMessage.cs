using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public interface IActorMessage
    {
        Type GetType(); // default .NET implementation is fine

        object Message { get; set; }

        object ProcessingResult { get; set; }

        TaskCompletionSource<IActorMessage> Status { get; set; }
    }
}
