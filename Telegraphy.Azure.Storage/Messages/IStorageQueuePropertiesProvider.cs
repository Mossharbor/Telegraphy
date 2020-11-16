using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public interface IStorageQueuePropertiesProvider
    {
        TimeSpan? TimeToLive { get; set; }
        TimeSpan? InitialVisibilityDelay { get; set; }
        QueueRequestOptions Options { get; set; }
        OperationContext OperationContext { get; set; }
    }
}
