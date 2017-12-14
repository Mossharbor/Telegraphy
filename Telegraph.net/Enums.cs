using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public enum LocalConcurrencyType { ActorsOnThreadPool, OneActorPerThread, OneThreadAllActors, OneThreadPerActor, LimitedThreadCount };

    public enum MessageDispatchProcedureType { RoundRobin, BroadcastToAll, LeastBusy, RandomSelection }
}
