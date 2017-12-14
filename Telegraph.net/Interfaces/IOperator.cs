using System;
namespace Telegraphy.Net
{
    using System.Linq.Expressions;

    public interface IOperator : IActor
    {
        long ID {get;set;}

        bool IsAlive();

        void Kill();

        ulong Count { get; }

        ILocalSwitchboard Switchboard { get; set; }

        void AddMessage(IActorMessage msg);

        IActorMessage GetMessage();

        bool WaitTillEmpty(TimeSpan timeout);

        void Register<T>(Action<T> action);

        void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
            //where T : IActorMessage
            where K : IActor;

        void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler);
    }
}
