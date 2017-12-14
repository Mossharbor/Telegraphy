using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiningPhilosophers
{
    using Telegraphy.Net;

    public class EatingMessage : SimpleMessage<EatingMessage>
    {
    }

    public class ThinkingMessage : SimpleMessage<ThinkingMessage>
    {
    }

    public class PrintMessage : SimpleMessage<PrintMessage>
    {

    }

    public class AcquireChopstick : SimpleMessage<AcquireChopstick> { }
    
}
