using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong_AzureQueue
{
    using Telegraphy.Net;
    using Telegraphy.Azure;
    using PingPong;

    class Program
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

                Telegraph.Instance.Register<Pong>(threadPoolOpID, message => Console.WriteLine(System.Environment.NewLine + message));
                Telegraph.Instance.Register<Ping>(azureOperator);
            }
            catch(FailedRegistrationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.GetType().ToString() + ":" + ex.Message);
                Console.ResetColor();
                return;
            }

            System.Threading.Thread.Sleep(1000); // wait for pong to become alive
            Task<IActorMessage> pingTask = Telegraph.Instance.Ask(new Ping());

            pingTask.Wait();
            Console.WriteLine("Ping Was Sent... press Enter to exit");
            Console.ReadLine();
        }
    }
}
