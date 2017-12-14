using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingPong;
using Telegraphy.Net;
using Telegraphy.Azure;

namespace Ping_AzureQueue
{
    class Ping
    {
        static void Main(string[] args)
        {
            try
            {
                LocalOperator singleThreadOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors));
                AzureSmallMessageQueueOperator azureOperator = new AzureSmallMessageQueueOperator(null);

                Telegraph.Instance.MessageDispatchProcedure = MessageDispatchProcedureType.RoundRobin;
                long threadPoolOpID = Telegraph.Instance.Register(singleThreadOperator);
                long azureOperatorID = Telegraph.Instance.Register(azureOperator); //NOTE: this sets the operator ID (singleThreadOperator.ID)

                Telegraph.Instance.Register<Ping>(threadPoolOpID, message => Telegraph.Instance.Tell(new Pong()));
                Telegraph.Instance.Register<Pong>(azureOperator);
            }
            catch (FailedRegistrationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.GetType().ToString() + ":" + ex.Message);
                Console.ResetColor();
                return;
            }
            
            Console.WriteLine("Waiting For Ping ... press Enter to exit");
            Console.ReadLine();
        }
    }
}
