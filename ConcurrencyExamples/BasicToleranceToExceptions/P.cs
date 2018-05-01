using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicToleranceToFailure
{
    using Telegraphy.Net;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            LogExceptions();

            RetryMessage();

            RestartActorAndReprocess();

            KillGroupsOfMessages();

            KillGroupOfMessagesOnFailure();

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        static void LogExceptions()
        {
            System.Diagnostics.Debug.WriteLine("LogExceptions");
            Telegraph.Instance.MainOperator = new LocalOperator(); // performs a reset.
            string messageStr = "LogExceptions.";

            // this is using the sequential local concurrency type which will create a new Actor for each message.
            DefaultActor da = new DefaultActor();
            da.OnMessageHandler = delegate(IActorMessage s) { throw new NullReferenceException(); };

            Telegraph.Instance.Register<byte[], DefaultActor>(() => da);

            Telegraph.Instance.Register(typeof(NullReferenceException), HandleExceptionWithPrintingIt);

            for (int i = 0; i < 10; ++i)
            {
                IActorMessage msg = new ValueTypeMessage<byte>(Encoding.ASCII.GetBytes(messageStr + i.ToString()));

                Telegraph.Instance.Tell(msg);
            }

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void RetryMessage()
        {
            System.Diagnostics.Debug.WriteLine("RetryMessage");
            Telegraph.Instance.MainOperator = new LocalOperator(); // performs a reset.
            string messageStr = "RetryMessage.";

            // this is using the sequential local concurrency type which will create a new Actor for each message.
            long count = 0;
            DefaultActor da = new DefaultActor();
            da.OnMessageHandler = delegate(IActorMessage s)
            {
                Interlocked.Increment(ref count);

                if (count == 1)
                    throw new ArgumentException("Testing here");
                else
                    Console.WriteLine("Handled " + (string)s.Message);

                return true;
            };

            Telegraph.Instance.Register<string, DefaultActor>(() => da);

            Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> errhandler = delegate(Exception ex, IActor actor, IActorMessage msg, IActorInvocation invoker)
            {
                // send the msg back to the queue
                Console.WriteLine("Sending msg back to the queue.");
                Telegraph.Instance.Tell(msg);

                return null;
            };

            // fallback to this exception since we didnt find a null ref exception
            Telegraph.Instance.Register(typeof(Exception), errhandler);

            for (int i = 0; i < 10; ++i)
            {
                IActorMessage msg = new SimpleMessage<string>(messageStr + i.ToString());

                Telegraph.Instance.Tell(msg);
            }

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 3, 0));
        }

        static void RestartActorAndReprocess()
        {
            System.Diagnostics.Debug.WriteLine("RetryMessage");
            Telegraph.Instance.MainOperator = new LocalOperator(); // performs a reset.
            string messageStr = "RetryMessage.";

            // this is using the sequential local concurrency type which will create a new Actor for each message.
            DefaultActor da = new DefaultActor();
            da.OnMessageHandler = delegate(IActorMessage s)
            {
                throw new ArgumentException("Testing here");
            };

            Telegraph.Instance.MainOperator.Register<string, DefaultActor>(() => da);

            Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> errhandler = delegate(Exception ex, IActor actor, IActorMessage msg, IActorInvocation invoker)
            {
                Console.WriteLine("Creating New Actor");
                DefaultActor da2 = new DefaultActor();
                da2.OnMessageHandler = delegate(IActorMessage s)
                {
                    Console.WriteLine("New Actor:"+(string)s.Message);
                    return true;
                };

                // send the msg back to the queue
                Console.WriteLine("Sending msg back to the queue.");
                Telegraph.Instance.Tell(msg);

                return da2;
            };

            // fallback to this exception since we didnt find a null ref exception
            Telegraph.Instance.Register(typeof(Exception), errhandler);

            IActorMessage origMessage = new SimpleMessage<string>(messageStr);
            Telegraph.Instance.Tell(origMessage);

            System.Threading.Thread.Sleep(1000);

            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0, 3, 0));
        }

        static void KillGroupsOfMessages()
        {
            System.Diagnostics.Debug.WriteLine("MessageCancelled");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread)); // performs a reset.
            Telegraph.Instance.Register<string>(message =>
            {
                System.Threading.Thread.Sleep(4000);
                Console.WriteLine(message + " bar");
            });

            DateTime start = DateTime.Now;

            TaskCompletionSource<IActorMessage> cancelToken = new TaskCompletionSource<IActorMessage>();
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Token.Register(() => cancelToken.TrySetCanceled());

            List<Task<IActorMessage>> tasks = new List<Task<IActorMessage>>();

            for (int i = 0; i < 10; ++i)
            {
                tasks.Add(Telegraph.Instance.Ask(cancelToken,"foo"));
            }

            System.Threading.Thread.Sleep(2000);

            try
            {
                // cancel all the tasks
                cancelToken.SetCanceled();
                //task.Wait();
            }
            catch (AggregateException e)
            {
                var found = e.Flatten().InnerExceptions.FirstOrDefault(p => p is TaskCanceledException);

                if (null != found)
                    Console.WriteLine("All tasks were Canceled.");
            }

            // NOTE only one task should have completed the one that was started before we called canceled.
            Console.WriteLine("MessageCancelled Task Completed in " + (DateTime.Now - start).TotalSeconds.ToString("00") + " seconds.");

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static void KillGroupOfMessagesOnFailure()
        {
            Random rand = new Random();
            System.Diagnostics.Debug.WriteLine("MessageCancelled");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread)); // performs a reset.
            Telegraph.Instance.Register<string>(message =>
            {
                int sleepTime = 1000 * rand.Next(20);

                Console.WriteLine(sleepTime + " milliseconds sleeping");
                System.Threading.Thread.Sleep(sleepTime);
                Console.WriteLine(message + " throwing exception.");
                throw new NotImplementedException("Throwing an exception");
            });

            Telegraph.Instance.Register(typeof(NotImplementedException), HandleExceptionByCancelingTask);

            DateTime start = DateTime.Now;

            // Since we are using the same CancellationTokenSource for each message we can cancel all of the messages
            // by cancelling one messages task.
            TaskCompletionSource<IActorMessage> cancelToken = new TaskCompletionSource<IActorMessage>();
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Token.Register(() => cancelToken.TrySetCanceled());

            List<Task<IActorMessage>> tasks = new List<Task<IActorMessage>>();

            for (int i = 0; i < 10; ++i)
            {
                tasks.Add(Telegraph.Instance.Ask(cancelToken, i.ToString()));
            }

            try
            {
                // cancel all the tasks
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e)
            {
                // Task cancelation

                var found = e.Flatten().InnerExceptions.FirstOrDefault(p => p is TaskCanceledException);

                if (null != found)
                    Console.WriteLine("All tasks were Canceled.");
                else
                    Console.WriteLine("Tasks were not cannceled.");
            }

            // NOTE only one task should have completed the one that was started before we called canceled.
            Console.WriteLine("MessageCancelled Task Completed in " + (DateTime.Now - start).TotalSeconds.ToString("00") + " seconds.");

            Telegraph.Instance.Ask(new ControlMessages.HangUp()).Wait();
        }

        static IActor HandleExceptionByCancelingTask(Exception ex, IActor actor, IActorMessage msg, IActorInvocation invoker)
        {
            if (msg.Message is string)
                System.Console.WriteLine("Null Reference Exception Handled for " + (string)msg.Message);

            if (null != msg.Status)
                msg.Status.SetCanceled();

            return null;
        }

        static IActor HandleExceptionWithPrintingIt(Exception ex, IActor actor, IActorMessage msg, IActorInvocation invoker)
        {
            if (msg.Message is byte[])
                System.Console.WriteLine(ex.GetType()+" Handled for " + Encoding.ASCII.GetString((byte[])msg.Message));

            if (null != msg.Status)
                msg.Status.SetException(ex);

            // NOTE we could instantiate a new actor here using the original Func that registered the action.  This is done through the passed in invoker
            // and return it for future processing.
            return null;
        }

    }
}
