using System;
namespace Telegraphy.Net
{
    public interface IActorInvocation
    {
        IActor Invoke();
        void Reset();
    }
}
