using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolePlayingGame
{
    using Telegraphy.Net;

    enum DragonState { Dead, Alive }

    class Dragon : IActor
    {
        Random rand = new Random();
        private int hitPoints = 150;

        public Dragon()
        {
            this.DragonState = RolePlayingGame.DragonState.Alive;
        }

        public DragonState DragonState { get; set; }

        public bool OnMessageRecieved<T>(T msg) 
            where T : IActorMessage
        {
            if (this.DragonState == RolePlayingGame.DragonState.Dead)
                return true;

            int damage = 0;
            if (msg is Attack)
                damage = (msg as Attack).Damage;
            else
                throw new NotImplementedException();

            bool isHit = (rand.Next() % 2 == 0);

            if (isHit)
            {
                Telegraph.Instance.Tell(new PrintHit((msg as Attack), "Dragon"));
                hitPoints -= damage;

                if (hitPoints <= 0)
                {
                    Telegraph.Instance.Tell(new PrintDeath("Dragon"));
                    this.DragonState = RolePlayingGame.DragonState.Dead;
                    return true;
                }
            }
            else
                Telegraph.Instance.Tell(new PrintMiss());

            bool useBreath = (rand.Next() % 2 == 0);
            Attack attack = null;

            if (useBreath)
                attack = new Breath();
            else
                attack = new Claw();

            Telegraph.Instance.Tell(attack);
            Telegraph.Instance.Tell(new PrintAttack(attack, "Dragon"));

            return true;
        }
    }
}
