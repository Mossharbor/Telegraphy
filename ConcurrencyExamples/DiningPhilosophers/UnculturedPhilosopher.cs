using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiningPhilosophers
{
    using Telegraphy.Net;

    /// <summary>
    /// These are uncultured philosphers because they dont use utensils to eat wtih.
    /// </summary>
    public class UnculturedPhilosopher : DefaultActor
    {
        public UnculturedPhilosopher() : base ()
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
