using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public enum LocalConcurrencyType { ActorsOnThreadPool, OneActorPerThread, OneThreadAllActors, OneThreadPerActor, DedicatedThreadCount };

    public enum MessageDispatchProcedureType { RoundRobin, BroadcastToAll, LeastBusy, RandomSelection }

    public enum MessageSource { ByteArrayMessage = 0, StringMessage = 1, EntireIActor = 2 }
}
