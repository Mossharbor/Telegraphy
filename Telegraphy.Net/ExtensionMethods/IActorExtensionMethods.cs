﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class IActorExtensionMethods
    {
        #region Ask and Tell
        public static bool Tell<T>(this IActor self, T message) where T : class
        {
            if (message is IActorMessage)
                return self.OnMessageRecieved((IActorMessage)message);
            
            return self.OnMessageRecieved(new SimpleMessage<T>(message));
        }

        public static Task<IActorMessage> Ask<T>(this IActor self, TaskCompletionSource<IActorMessage> cancelToken, T message) where T : class
        {
            IActorMessage asker = null;
            if (message is IActorMessage)
            {
                asker = (IActorMessage)message;
                ((IActorMessage)message).Status = cancelToken;
            }
            else
                asker = new AnonAskMessage<T>(message, cancelToken);

            self.OnMessageRecieved(asker);
            return cancelToken.Task;
        }

        public static bool Tell(this IActor self, IActorMessage message)
        {
            return self.OnMessageRecieved(message);
        }

        public static Task<IActorMessage> Ask<T>(this IActor self, T message) where T : class
        {
            TaskCompletionSource<IActorMessage> dummyToken;
            return self.Ask<T>(message, null, out dummyToken);
        }

        public static Task<IActorMessage> Ask<T>(this IActor self, T message, TimeSpan? timeout) where T : class
        {
            TaskCompletionSource<IActorMessage> dummyToken;
            return self.Ask<T>(message, timeout, out dummyToken);
        }

        public static Task<IActorMessage> Ask<T>(this IActor self, T message, out  TaskCompletionSource<IActorMessage> cancelToken) where T : class
        {
            return self.Ask<T>(message, null,out cancelToken);
        }

        public static Task<IActorMessage> Ask<T>(this IActor self, T message, TimeSpan? timeout, out TaskCompletionSource<IActorMessage> cancelToken) where T : class
        {
            cancelToken = new TaskCompletionSource<IActorMessage>();
            GetMessageTask(timeout, cancelToken);

            return IActorExtensionMethods.Ask<T>(self, cancelToken, message);
        }

        #endregion

        private static void GetMessageTask(TimeSpan? timeout, TaskCompletionSource<IActorMessage> cancelToken)
        {
            if (timeout.HasValue)
            {
                var cancellationSource = new CancellationTokenSource();
                cancellationSource.Token.Register(() => cancelToken.TrySetCanceled());
                cancellationSource.CancelAfter(timeout.Value);
            }
        }
    }
}
