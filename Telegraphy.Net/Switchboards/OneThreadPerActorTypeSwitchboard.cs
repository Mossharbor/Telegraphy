﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace Telegraphy.Net
{
    public class OneThreadPerActorTypeSwitchboard : ILocalSwitchboard
    {
        protected long tellingCount = 0;
        private ConcurrentDictionary<Type, ILocalSwitchboard> _actorToSwitchBoard = new ConcurrentDictionary<Type, ILocalSwitchboard>();
        private ConcurrentDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>>();
        
        public LocalConcurrencyType LocalConcurrencyType { get; private set; }

        public uint Concurrency { get; private set; }

        private IOperator myOperator = null;
        public IOperator Operator
        {
            get { return myOperator; }
            set
            {
                foreach (var t in _actorToSwitchBoard)
                    t.Value.Operator = value;
                myOperator = value;
            }
        }
        
        private void ShutdownSwitchboards()
        {
            foreach(var t in _actorToSwitchBoard)
            {
                t.Value.Disable();
            }
        }

        public void Disable()
        {
            ShutdownSwitchboards();
        }

        public bool IsDisabled()
        {
            foreach (var t in _actorToSwitchBoard)
            {
                bool isDisabled = t.Value.IsDisabled();
                if (!isDisabled)
                    return false;
            }
            return true;
        }

        public void Register<T>(Action<T> action) where T : class
        {
            IActor anonActor = new AnonActor<T>(action);
            ILocalSwitchboard switchBoard = new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread);
            switchBoard.Operator = this.Operator;
            _actorToSwitchBoard.TryAdd(anonActor.GetType(), switchBoard);

            switchBoard.Register(action);
            foreach (var t in _exceptionTypeToHandler)
                switchBoard.Register(t.Key, t.Value);
            // for each T we create a new LocalSwitchboard that only operates on that type T
        }

        public void Register<T>(IActor actor) where T : class, IActorMessage
        {
            this.Register<T>((msg) => actor.OnMessageRecieved<T>(msg));
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
            Type typeOfActor = typeof(K);

            if (_actorToSwitchBoard.ContainsKey(typeOfActor))
            {
                throw new ActorHasAlreadyBeenRegisteredException();
            }
            ILocalSwitchboard switchBoard = new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread);
            switchBoard.Operator = this.Operator;
            _actorToSwitchBoard.TryAdd(typeOfActor, switchBoard);
            switchBoard.Register<T,K>(factory);
            foreach (var t in _exceptionTypeToHandler)
                switchBoard.Register(t.Key, t.Value);
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            _exceptionTypeToHandler.TryAdd(exceptionType, handler);
            foreach (var t in _actorToSwitchBoard.Values)
            {
                t.Register(exceptionType, handler);
            }
        }

        public void Register<T>()
        {
            throw new SwitchBoardRequiresARegisteredActorOrActionException();
        }

        public void Register<T>(string registrationString)
        {
            throw new SwitchBoardRequiresARegisteredActorOrActionException();
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (null == this.Operator)
                return false;

            this.Operator.AddMessage(msg);
            return true;
        }
    }
}
