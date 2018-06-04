using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RolePlayingGame
{
    using Telegraphy.Net;

    class Program
    {
        static void Main(string[] args)
        {
            Telegraph.Instance.Register(new LocalQueueOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread)));

            Knight player = new Knight();
            Dragon dragon = new Dragon();

            Telegraph.Instance.Register<Claw, Knight>(() => player);
            Telegraph.Instance.Register<Breath, Knight>(() => player);

            Telegraph.Instance.Register<Arrow, Dragon>(() => dragon);
            Telegraph.Instance.Register<BattleAxe, Dragon>(() => dragon);

            Telegraph.Instance.Register<PrintAttack, Display>(() => new Display());
            Telegraph.Instance.Register<PrintHit, Display>(() => new Display());
            Telegraph.Instance.Register<PrintMiss, Display>(() => new Display());
            Telegraph.Instance.Register<PrintDeath, Display>(() => new Display());
            
            // knight attacks dragon first which starts a battle
            Telegraph.Instance.Tell(new PrintAttack(new Arrow(),"Knight"));
            Telegraph.Instance.Tell(new Arrow());

            Console.ReadLine();
        }
    }
}
