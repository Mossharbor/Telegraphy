﻿using System;
namespace Telegraphy.Net
{
    using System.Collections.Generic;

    public interface IOperator : IActor
    {
        long ID {get;set;}

        void Kill();

        ulong Count { get; }

        ICollection<ILocalSwitchboard> Switchboards { get; }

        void AddMessage(IActorMessage msg);

        IActorMessage GetMessage();

        bool WaitTillEmpty(TimeSpan timeout);

        //void Register<T>(Action<T> action) where T : class;

        //void Register<T, K>(System.Linq.Expressions.Expression<Func<K>> factory)
        //    where T : class
        //    where K : IActor;

        void Register(Type exceptionType, Action<Exception> handler);
    }
}
