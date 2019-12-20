using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    using System.Linq.Expressions;
    using Telegraphy.Net;

    public class TestSwitchBoard : ILocalSwitchboard
    {
        public uint Concurrency { get; set; }

        public LocalConcurrencyType LocalConcurrencyType { get; set; }

        public IOperator Operator { get; set; }

        public void Disable()
        {
        }

        public bool IsDisabled()
        {
            return false;
        }

        public void Register<T>(Action<T> action) where T : class
        {
        }

        public void Register<T>(IActor actor) where T : class, IActorMessage
        {
        }

        public void Register<T, K>(Expression<Func<K>> factory) where K : IActor
        {
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
        }

        public void Register<T>()
        {
        }

        public void Register<T>(string registrationString)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            return true;
        }
    }
}
