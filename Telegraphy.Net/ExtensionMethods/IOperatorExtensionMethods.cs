using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public static class IOperatorExtensionMethods
    {
        public static Action<Exception> FindExceptionHandler(this IOperator me, IDictionary<Type, Action<Exception>> _exceptionTypeToHandler, Exception ex, out Exception foundEx)
        {
            Action<Exception> handler;
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
                return null;
            }

            return handler;
        }
    }
}
