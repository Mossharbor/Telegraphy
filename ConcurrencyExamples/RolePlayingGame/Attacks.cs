using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolePlayingGame
{
    using Telegraphy.Net;
    public class Attack : IActorMessage
    {
        public Attack(int damage)
        {
            this.Damage = damage;
        }

        public int Damage { get;set; }

        public object Message { get { return Damage; } set { this.Damage = (int)value; } }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }
    }

    public class BattleAxe : Attack
    {
        public BattleAxe() : base(8) { }
    }

    public class Arrow : Attack
    {
        public Arrow() : base(2) { }
    }

    public class Claw : Attack
    {
        public Claw() : base(6) { }
    }

    public class Breath : Attack
    {
        public Breath() : base(18) { }
    }
}
