using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicStartHere
{
    using Telegraphy.Net;
    using System.Threading.Tasks;
    using System.Threading;

    public class P
    {
        public static void Main(string[] args)
        {
            //HelloWorld();

            //HelloWorld2();

            //SimpleSingleThreadSequential();

            //ThreadPool();

            //LimitedThreadPool();

            //WorkerThreads();

            //LazyInstantiation();

            //LazyInstantiation2();

            //WaitOnMultipleMessagesToComplete();

            //WaitForCompletion();

            //MessageTimeOut();

            //MessageCancelled();

            //GetResultOfProcessing();

            //MessageOrdering();

            //MessageOrdering2();

            //BasicMessageSerializationDeserialization();

            //ComplexMessageSerializationDeserialization();

            //MultipleOperatorsBasic();

            //BroadcastToAllOperators();

            ThrottlingIncomingMessages();

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        public static void HelloWorld()
        {
            Telegraph.Instance.Register<string>(message => Console.WriteLine(message));

            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell("Hello World." + i.ToString());

            Telegraph.Instance.Ask(new HangUp()).Wait();
        }

        public static void HelloWorld2()
        {
            string messageStr = "HelloWorld2.";
            try
            {
                Telegraph.Instance.MainOperator = new LocalOperator(); // performs a reset when we assign a new operator
                Telegraph.Instance.Register<byte[], LazyInstantiationActor>(() => new LazyInstantiationActor());
            }
            catch (FailedRegistrationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.GetType().ToString() + ":" + ex.Message);
                Console.ResetColor();
                return;
            }

            for (int i = 0; i < 10; ++i)
            {
                IActorMessage msg = new SimpleMessage<byte[]>(Encoding.ASCII.GetBytes(messageStr + i.ToString()));

                Telegraph.Instance.Tell(msg);
            }

            Telegraph.Instance.Ask(new HangUp()).Wait();
        }

        public static void SimpleSingleThreadSequential()
        {
            try
            {
                Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadPerActor));

                Telegraph.Instance.Register<string>(message =>
                {
                    Console.WriteLine(message);
                });

                Telegraph.Instance.Tell("SimpleSingleThreadSequential");

                for (int i = 0; i < 10; ++i)
                    Telegraph.Instance.Tell("SimpleSingleThreadSequential " + i.ToString());
            }
            catch (FailedRegistrationException ex)
            {
                Console.Error.WriteLine(ex.GetType().ToString()+":"+ex.Message);
                return;
            }

            Task task = Telegraph.Instance.Ask(new HangUp());
            task.Wait();
        }

        public static void ThreadPool()
        {
            Telegraph.Instance.MainOperator = new LocalOperator();
            Telegraph.Instance.MainOperator.Switchboard = new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool);

            Telegraph.Instance.Register<string>(message =>
            {
                Console.WriteLine(message);
            });

            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell("ThreadPool was executed." + i.ToString());

            Telegraph.Instance.MainOperator.WaitTillEmpty(new TimeSpan(1, 0, 0));
        }

        public static void LimitedThreadPool()
        {
            var localOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool, 2));
            Telegraph.Instance.MainOperator = localOperator;

            Telegraph.Instance.Register<string>(message =>
            {
                Console.WriteLine(message);
            });

            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell("LimitedThreadPool was executed." + i.ToString());

            Telegraph.Instance.MainOperator.WaitTillEmpty(new TimeSpan(1, 0, 0));
        }

        public static void WorkerThreads()
        {
            System.Diagnostics.Debug.WriteLine("WorkerThreads");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread, 20));

            Telegraph.Instance.Register<string>(message =>
            {
                Console.WriteLine(message);
            });

            for (int i = 0; i < 10; ++i)
            {
                Telegraph.Instance.Tell("Worker was executed." + i.ToString());
            }

            Telegraph.Instance.MainOperator.WaitTillEmpty(new TimeSpan(1, 0, 0));
        }

        public static void LazyInstantiation()
        {
            System.Diagnostics.Debug.WriteLine("LazyInstantiation");
            Telegraph.Instance.MainOperator = new LocalOperator(); // performs a reset.
            string messageStr = "LazyInstantiationActor.";

            Telegraph.Instance.Register<byte[]>(message =>
            {
                LazyInstantiationActor la = new LazyInstantiationActor();
                la.Tell(message);
            });

            for (int i = 0; i < 10; ++i)
                Telegraph.Instance.Tell(Encoding.ASCII.GetBytes(messageStr + i.ToString()));

            Telegraph.Instance.MainOperator.WaitTillEmpty(new TimeSpan(0, 0, 20));
        }

        public static void LazyInstantiation2()
        {
            System.Diagnostics.Debug.WriteLine("LazyInstantiation2");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.
            string messageStr = "LazyInstantiation2.";

            // this is using the local threadpool concurrency type which will create a new Actor for each message.
            Telegraph.Instance.Register<byte[], LazyInstantiationActor>(() => new LazyInstantiationActor());

            for (int i = 0; i < 10; ++i)
            {
                Telegraph.Instance.Tell(Encoding.ASCII.GetBytes(messageStr + i.ToString()));
            }

            Telegraph.Instance.MainOperator.WaitTillEmpty(new TimeSpan(1, 0, 0));
        }

        public static void WaitOnMultipleMessagesToComplete()
        {
            System.Diagnostics.Debug.WriteLine("WaitOnMultipleMessagesToComplete");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread, 20)); // performs a reset.
            string messageStr = "WaitOnMultipleMessagesToComplete.";

            LazyInstantiationActor lzyActor = new LazyInstantiationActor();
            Func<IActorMessage,bool> writeFcn = delegate(IActorMessage s) { lzyActor.Print(s); return true; };

            // this is using the local threadpool concurrency type which will create a new Actor for each message.
            Telegraph.Instance.Register<byte[], DefaultActor>(() => new DefaultActor(writeFcn));

            for (int i = 0; i < 10; ++i)
            {
                Telegraph.Instance.Tell(Encoding.ASCII.GetBytes(messageStr + i.ToString()));
            }

            Task<IActorMessage>[] tasksToWaitOn = new Task<IActorMessage>[20] ;
            for (int i = 0; i < 20; ++i)
            {
                tasksToWaitOn[i] = Telegraph.Instance.Ask(new HangUp());
            }

            Task.WaitAll(tasksToWaitOn);
        }

        public static void WaitForCompletion()
        {
            System.Diagnostics.Debug.WriteLine("WaitForCompletion");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.
            Telegraph.Instance.Register<string>(message =>
            {
                System.Threading.Thread.Sleep(4000);
                Console.WriteLine(message + " bar");
            });

            DateTime start = DateTime.Now;
            Console.WriteLine("Waiting for Task Completion");
            var task = Telegraph.Instance.Ask("Foo");
            task.Wait();
            Console.WriteLine("WaitForCompletion Task Completed in " + (DateTime.Now - start).TotalSeconds.ToString("00") + " seconds.");
        }

        public static void MessageTimeOut()
        {
            System.Diagnostics.Debug.WriteLine("MessageTimeOut");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread)); // performs a reset.
            Telegraph.Instance.Register<string>(message =>
            {
                System.Threading.Thread.Sleep(4000);
                Console.WriteLine(message + " bar");
            });

            TimeSpan timeout = new TimeSpan(0, 0, 2); // death if we take longer than 20 seconds
            DateTime start = DateTime.Now;
            var task = Telegraph.Instance.Ask("Foo", timeout);

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                var found = e.Flatten().InnerExceptions.FirstOrDefault(p=>p is TaskCanceledException);

                if (null != found)
                    Console.WriteLine("Task Canceled because it timed out.");
            }

            Console.WriteLine("MessageTimeOut Completed in " + (DateTime.Now - start).TotalSeconds.ToString("00") + " seconds.");
        }

        public static void MessageCancelled()
        {
            System.Diagnostics.Debug.WriteLine("MessageCancelled");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread)); // performs a reset.
            Telegraph.Instance.Register<string>(message =>
            {
                System.Threading.Thread.Sleep(4000);
                Console.WriteLine(message + " bar");
            });

            TimeSpan timeout = new TimeSpan(0, 0, 2); // death if we take longer than 20 seconds
            DateTime start = DateTime.Now;

            TaskCompletionSource<IActorMessage> cancelToken;

            var task = Telegraph.Instance.Ask("Foo", out cancelToken);

            try
            {
                cancelToken.SetCanceled();
                task.Wait();
            }
            catch (AggregateException e)
            {
                var found = e.Flatten().InnerExceptions.FirstOrDefault(p => p is TaskCanceledException);

                if (null != found)
                    Console.WriteLine("Task was Canceled.");
            }

            Console.WriteLine("MessageCancelled Task Completed in " + (DateTime.Now - start).TotalSeconds.ToString("00") + " seconds.");
        }

        public static void GetResultOfProcessing()
        {
            System.Diagnostics.Debug.WriteLine("GetResultOfProcessing");
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors)); // performs a reset.
            Telegraph.Instance.Register<Compute>(compute =>
            {
                compute.RaiseToPower(2);
            });

            var task = Telegraph.Instance.Ask(new Compute(4));
            task.Wait();

            if (task.IsCompleted)
            {
                double result = (double)(task.Result.ProcessingResult);

                Console.WriteLine("GetResultOfProcessing returned " + result.ToString("00"));
            }
        }

        public static void MessageOrdering()
        {
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadPerActor)); // performs a reset.
            Telegraph.Instance.Register<string>(message =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(message + " bar");
            });

            Telegraph.Instance.Register<Compute>(compute =>
            {
                compute.RaiseToPower(2);
                System.Threading.Thread.Sleep(300);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Computed.");
                Console.ResetColor();
            });

            Task<IActorMessage> computeTask = null;

            for (int i = 0; i <= 75; ++i)
            {
                Telegraph.Instance.Tell("foo "+i.ToString());

                if (i == 35)
                    computeTask = Telegraph.Instance.Ask(new Compute(4));
            }

            computeTask.Wait();

            System.Threading.Thread.Sleep(10000);
            double result = (double)(computeTask.Result.ProcessingResult);
            Console.WriteLine("MessageOrdering compute task returned " + result.ToString("00"));
        }

        public static void MessageOrdering2()
        {
            System.Diagnostics.Debug.WriteLine("MessageOrdering2");
            Telegraph.Instance.MainOperator = new SingleThreadPerMessageTypeOperator();

            Telegraph.Instance.Register<string>(message =>
            {
                System.Threading.Thread.Sleep(300);
                Console.WriteLine(message + " bar");
            });

            Telegraph.Instance.Register<Compute>(compute =>
            {
                compute.RaiseToPower(2);
                Console.WriteLine("computed.");
            });

            // NOTE foo1 foo2 foo3 foo4 are guaranteed to be delivered in order to a single thread.
            var computeTask = Telegraph.Instance.Ask(new Compute(4));
            var printTask1 = Telegraph.Instance.Ask("2 - foo 1");
            var printTask2 = Telegraph.Instance.Ask("2 - foo 2");
            var printTask3 = Telegraph.Instance.Ask("2 - foo 3");
            var printTask4 = Telegraph.Instance.Ask("2 - foo 4");

            computeTask.Wait();
            printTask1.Wait();
            printTask2.Wait();
            printTask3.Wait();
            printTask4.Wait();

            double result = (double)(computeTask.Result.ProcessingResult);
            Console.WriteLine("MessageOrdering2 compute task returned " + result.ToString("00"));
        }

        public static void BasicMessageSerializationDeserialization()
        {
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.

            Telegraph.Instance.Register<string>(message => Console.WriteLine("BasicMessageSerializationDeserialization: "+message));
            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => new MessageSerializationActor());
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(() => new MessageDeserializationActor());

            var processingTask = Telegraph.Instance.Ask("foo");
            processingTask.Wait();

            SerializeMessage serializeRqst = new SerializeMessage(processingTask.Result);

            var serializeTask = Telegraph.Instance.Ask(serializeRqst);
            serializeTask.Wait();

            //NOTE you could also do the following to avoid having to register above
            //var serializeTask = (new MessageSerializationActor()).Ask(serializeRqst);
            //serializeTask.Wait();

            byte[] serializedBytes = (serializeTask.Result.ProcessingResult as byte[]);

            DeSerializeMessage msgToSerialize2 = new DeSerializeMessage(serializedBytes);
            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            string output = (task.Result.Message as string);

            System.Diagnostics.Debug.Assert(task.Result.GetType() == typeof(SerializeMessage));

            Console.WriteLine("Serialized and De-serialized " + output);
        }

        public static void ComplexMessageSerializationDeserialization()
        {
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.

            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => new MessageSerializationActor());
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(() => new MessageDeserializationActor());

            CustomSerializableMessage originalMsg = new CustomSerializableMessage(100,"Foo");
            SerializeMessage serializeRqst = new SerializeMessage(originalMsg);

            var serializeTask = Telegraph.Instance.Ask(serializeRqst);
            serializeTask.Wait();

            //NOTE you could also do the following to avoid having to register above
            //var serializeTask = (new MessageSerializationActor()).Ask(serializeRqst);
            //serializeTask.Wait();

            byte[] serializedBytes = (serializeTask.Result.ProcessingResult as byte[]);

            DeSerializeMessage msgToSerialize2 = new DeSerializeMessage(serializedBytes);
            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            CustomSerializableMessage output = (task.Result.Message as CustomSerializableMessage);

            Console.WriteLine("Serialized " + output.MyProperty.ToString());
        }

        public static void MultipleOperatorsBasic()
        {
            try
            {
                LocalOperator threadPoolOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool));
                LocalOperator singleThreadOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors));

                Telegraph.Instance.MessageDispatchProcedure = MessageDispatchProcedureType.RoundRobin;
                long threadPoolOpID = Telegraph.Instance.Register(threadPoolOperator);
                long singleThreadOperatorID = Telegraph.Instance.Register(singleThreadOperator); //NOTE: this sets the operator ID (singleThreadOperator.ID)

                Telegraph.Instance.Register<string>(threadPoolOpID, message => Console.WriteLine(System.Environment.NewLine+message));
                Telegraph.Instance.Register<int>(threadPoolOpID, count => Console.Write(count+","));
            }
            catch (FailedRegistrationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.GetType().ToString() + ":" + ex.Message);
                Console.ResetColor();
                return;
            }

            List<Task<IActorMessage>> msgsToWaitOn = new List<Task<IActorMessage>>();

            for (int i = 0; i < 100; ++i)
            {
                if (i % 10 == 0)
                {
                    // these will happen at random since we are running strings on the thread pool
                    Telegraph.Instance.Tell<string>("MultipleOperatorsBasic:" + i.ToString());
                }

                // this should be sequential since we are on one thread for ints
                msgsToWaitOn.Add(Telegraph.Instance.Ask<int>(i));
            }

            Task.WaitAll(msgsToWaitOn.ToArray());

            Console.WriteLine(System.Environment.NewLine + "MultipleOperatorsBasic finished");
        }

        public static void BroadcastToAllOperators()
        {
            try
            {
                LocalOperator op1 = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors));
                LocalOperator op2 = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors));

                Telegraph.Instance.MessageDispatchProcedure = MessageDispatchProcedureType.RoundRobin;
                long op1ID = Telegraph.Instance.Register(op1);
                long op2ID = Telegraph.Instance.Register(op2); //NOTE: this sets the operator ID (op2.ID)

                // register a string for two different operators
                Telegraph.Instance.Register<string>(op1ID, message => Console.WriteLine("1." + message));
                Telegraph.Instance.Register<string>(op2ID, message => Console.WriteLine("2." + message));
            }
            catch (FailedRegistrationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.GetType().ToString() + ":" + ex.Message);
                Console.ResetColor();
                return;
            }

            for (int i = 0; i < 10; ++i)
            {
                // this should be sequential since we are on one thread for ints
                Telegraph.Instance.Broadcast<string>(i.ToString());
            }

            //Task.WaitAll(msgsToWaitOn.ToArray());
            Telegraph.Instance.WaitTillEmpty(new TimeSpan(0,1,0));

            Console.WriteLine(System.Environment.NewLine + "MultipleOperatorsBasic finished");
        }

        public static void ThrottlingIncomingMessages()
        {
            AsyncSemaphore messageThrottle = new AsyncSemaphore(2, 2); // allow 2 at a time so we dont bombard the thread pool

            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.

            Telegraph.Instance.Register<string>(message => {
                Console.WriteLine("ThrottlingIncomingMessages: " + message);
                System.Threading.Thread.Sleep(100);
            });

            for(int i=0; i < 100; ++i)
            {
                messageThrottle.Queue(()=>{
                    Task question = Telegraph.Instance.Ask("foo");
                    question.Wait();
                });
            }

            while (0 != messageThrottle.CurrentCount)
                System.Threading.Thread.Sleep(1000);
        }
    }
}
