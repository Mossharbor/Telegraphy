using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiningPhilosophers
{
    using Telegraphy.Net;
    using System.Threading.Tasks;

    public enum PhilosopherType { ThinkingPhilosopher, EatingPhilosopher };

    class P
    {
        static void BasicDiners()
        { 
            // We can model this much of a philosopher’s behavior with the following behavior definitions
            //These will fail, due to the LocalOperator does not enforce one message at a time per actor
            //var localOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ThreadPool));
            //var localOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.PreAllocatedThreads, 10));

            // This will not work because we have the same Philosopher processing messages on both threads. 
            //var localOperator = new SingleThreadPerMessageTypeOperator();

            // this only processes on a single thread so this will succeed because message execution order is 
            // guaranteed.  However this can be non-optimal for many scenarios with lots of Actors and messages.
            // since all actors and messages share the same thread.
            //var localOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.Sequential));

            var localOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadPerActor));

            Telegraph.Instance.Register(localOperator);
            var t = new UnculturedPhilosopher();
            Telegraph.Instance.Register<EatingMessage, UnculturedPhilosopher>(() => t);
            Telegraph.Instance.Register<ThinkingMessage, UnculturedPhilosopher>(() => t);
            Telegraph.Instance.Register<PrintMessage, UnculturedPhilosopher>(() => t);

            StartContimplativeGluttonousLoop(new TimeSpan(0, 0, 0,0,300)); // Generate messages for 50ms
        }

        static void CulturedDiners()
        {
            // modify the philosopher’s behavior to include a state transition through “hungry”, where chopsticks are obtained before eating.
            // the philosophers must send an AquireChopstick message to the InfiiteChopstick Actor (and receive a response) before they can eat.

            var localOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadPerActor));

            Telegraph.Instance.Register(localOperator);
            var t = new CulturedPhilosopher();
            Telegraph.Instance.Register<EatingMessage, CulturedPhilosopher>(() => t);
            Telegraph.Instance.Register<ThinkingMessage, CulturedPhilosopher>(() => t);
            Telegraph.Instance.Register<PrintMessage, CulturedPhilosopher>(() => t);
            Telegraph.Instance.Register<AcquireChopstick, InfiniteChopstickContainer>(() => new InfiniteChopstickContainer());

            StartContimplativeGluttonousLoop(new TimeSpan(0, 0, 0, 0, 10)); // Generate messages for 10ms
        }

        static void StartContimplativeGluttonousLoop(TimeSpan runTime)
        {
            DateTime start = DateTime.Now;
            TimeSpan elapsed;
            IActorMessage lastMessage = new ThinkingMessage();
            Random nextDelay = new Random();
            List<Task<IActorMessage>> tasksToWaitOn = new List<Task<IActorMessage>>();

            do
            {
                Telegraph.Instance.Tell(new PrintMessage());

                elapsed = DateTime.Now - start;

                if (lastMessage is ThinkingMessage)
                    lastMessage = new EatingMessage();
                else
                    lastMessage = new ThinkingMessage();

                tasksToWaitOn.Add(Telegraph.Instance.Ask(lastMessage));
            } while (elapsed < runTime);

            Task.WaitAll(tasksToWaitOn.ToArray());
        }

        static void Main(string[] args)
        {
            //http://www.dalnefre.com/wp/2010/08/dining-philosophers-in-humus/

            // We will model each philosopher as an actor. 
            // Each philosopher alternates between two major states,
            // “thinking” and “eating”. In the “thinking” state, a 
            // philosopher that receives an #eat message becomes hungry 
            // and wants to eat. In the “eating” state, a philosopher 
            // that receives a #think message will stop eating and begin thinking.

            //BasicDiners();

            CulturedDiners();

            Console.ReadLine();
        }
    }
}
