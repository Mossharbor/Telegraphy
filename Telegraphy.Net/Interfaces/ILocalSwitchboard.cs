using System;
namespace Telegraphy.Net
{
    public interface ILocalSwitchboard : IActor
    {
        uint Concurrency { get; }

        bool IsDisabled();

        void Disable();
        LocalConcurrencyType LocalConcurrencyType { get; }
        IOperator Operator { get; set; }

        void Register<T>(IActor action) where T : class, IActorMessage;

        void Register<T>(Action<T> action) where T : class;

        void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
            //where T : IActorMessage
            where K : IActor;

        void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler);
        void Register<T>();
        void Register<T>(string registrationString);
    }
}
