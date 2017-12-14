using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiningPhilosophers
{
     using Telegraphy.Net;

    /// <summary>
    /// These are cultured philosphers because they use utensils to eat wtih.
    /// </summary>
    class CulturedPhilosopher : DefaultActor
    {
        public CulturedPhilosopher()
            : base()
        {
            PhilosopherState = PhilosopherType.ThinkingPhilosopher;
        }

        public PhilosopherType PhilosopherState
        {
            get;
            set;
        }

        private void CheckStateNot(PhilosopherType notType)
        {
            if (notType == this.PhilosopherState)
            {
                Console.ForegroundColor=ConsoleColor.Red;
                Console.Error.WriteLine("State was " + notType + " and should not have been.");
                Console.ResetColor();
            }
        }

        private void AcquireChopsticks()
        {
            Task rightChopStick = Telegraph.Instance.Ask<AcquireChopstick>(new AcquireChopstick());
            Task leftChopStick = Telegraph.Instance.Ask<AcquireChopstick>(new AcquireChopstick());

            Task[] chopsticks = new Task[] { rightChopStick, leftChopStick };

            Task.WaitAll(chopsticks);
            Console.WriteLine("Chopsticks Acquired");
        }

        public override bool OnMessageRecieved<T>(T msg)
        {
            if (msg.GetType() == typeof(ThinkingMessage))
            {
                CheckStateNot(PhilosopherType.ThinkingPhilosopher);
                this.PhilosopherState = PhilosopherType.ThinkingPhilosopher;
                System.Threading.Thread.Sleep(100);
            }
            else if (msg.GetType() == typeof(EatingMessage))
            {
                CheckStateNot(PhilosopherType.EatingPhilosopher);
                AcquireChopsticks();
                this.PhilosopherState = PhilosopherType.EatingPhilosopher;
                System.Threading.Thread.Sleep(150);
            }
            else if (msg is PrintMessage)
            {
                Console.WriteLine("The Philospher is "+((PhilosopherType.EatingPhilosopher == this.PhilosopherState)? "Eating." :"Thinking."));
            }

            return true;
        }
    }
}
