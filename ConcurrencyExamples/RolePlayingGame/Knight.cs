using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RolePlayingGame
{
    using Telegraphy.Net;

    public enum PlayerState { Dead, Alive }
    public class Knight : IActor
    {
        Random rand = new Random();
        private int hitPoints = 100;

        public Knight()
        {
            PlayerState = RolePlayingGame.PlayerState.Alive;
        }

        public PlayerState PlayerState { get; set; }

        public bool OnMessageRecieved<T>(T msg) 
            where T : class, IActorMessage
        {
            if (this.PlayerState == RolePlayingGame.PlayerState.Dead)
                return true;

            int damage = 0;
            if (msg is Attack)
                damage = (msg as Attack).Damage;
            else
                throw new NotImplementedException();

            bool isHit = (rand.Next() % 6 == 0);

            if (isHit)
            {
                hitPoints -= damage;
                Telegraph.Instance.Tell(new PrintHit((msg as Attack), "Knight"));

                if (hitPoints <= 0)
                {
                    Telegraph.Instance.Tell(new PrintDeath("Knight"));
                    this.PlayerState = RolePlayingGame.PlayerState.Dead;
                    return true;
                }
            }
            else
                Telegraph.Instance.Tell(new PrintMiss());

            bool useBattleAxe = (rand.Next() % 2 == 0);
            Attack attack = null;

            if (useBattleAxe)
                attack = new BattleAxe();
            else
                attack = new Arrow();
            
            Telegraph.Instance.Tell(attack);
            Telegraph.Instance.Tell(new PrintAttack(attack,"Knight"));

            return false;
        }
    }
}
