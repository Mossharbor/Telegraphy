using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolePlayingGame
{
    using Telegraphy.Net;

    class Display : IActor
    {
        public bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
            try
            {
                if (msg is PrintAttack)
                {
                    Console.Write(msg.Message.ToString() + "...");
                }
                else
                {
                    if (msg is PrintHit)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (msg is PrintDeath)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (msg is PrintMiss)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    Console.WriteLine(msg.Message.ToString());
                }
                return true;
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }

    class PrintAttack : SimpleMessage<PrintAttack>
    {
        public PrintAttack(Attack attackType, string source)
        {
            this.Message = source + " attacks with " + attackType.GetType().ToString().Replace("RolePlayingGame.", "");
        }
    }

    class PrintHit : SimpleMessage<PrintHit>
    {
        public PrintHit(Attack attackType, string attackee)
        {
            this.Message = "Hit for " + attackType.Damage.ToString() + " points against " + attackee;
        }
    }

    class PrintMiss : SimpleMessage<PrintMiss>
    {
        public PrintMiss()
        {
            this.Message = "Miss";
        }
    }

    class PrintDeath : SimpleMessage<PrintDeath>
    {
        public PrintDeath(string whoDied)
        {
            this.Message = "The " + whoDied + " is dead.";
        }
    }
}
