using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public static class ISwitchBoardExtentionMethods
    {
        public static Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> FindExceptionHandler(this ILocalSwitchboard me, IDictionary<Type, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor>> _exceptionTypeToHandler, Exception ex, IActor actor, IActorMessage msg, out Exception foundEx)
        {
            Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler;
            foundEx = ex;
            bool found = false;
            do
            {
                if (_exceptionTypeToHandler.TryGetValue(foundEx.GetType(), out handler))
                {
                    found = true;
                    break;
                }
                foundEx = ex.InnerException;
            } while (null != ex.InnerException);

            if (!found)
                foundEx = ex;

            //Try the default exception handler
            if (!found && !_exceptionTypeToHandler.TryGetValue(typeof(Exception), out handler))
            {
                if (null != msg.Status)
                    msg.Status.SetException(ex);

                return null;
            }

            return handler;
        }
    }
}
