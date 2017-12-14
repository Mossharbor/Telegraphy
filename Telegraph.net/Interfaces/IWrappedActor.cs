using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    interface IWrappedActor
    {
        IActor OriginalActor { get; }
    }
}
