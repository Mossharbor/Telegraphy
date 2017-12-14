using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    using Telegraphy.Net;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    public class Serilization
    {
        public static TaskCompletionSource<IActorMessage> GetMessageCompletionMonitor()
        {
            return GetMessageCompletionMonitor(null);
        }

        public static TaskCompletionSource<IActorMessage> GetMessageCompletionMonitor(TimeSpan? timeout)
        {
            var cancelToken = new TaskCompletionSource<IActorMessage>();
            GetMessageTask(timeout, cancelToken);

            return cancelToken;
        }

        private static void GetMessageTask(TimeSpan? timeout, TaskCompletionSource<IActorMessage> cancelToken)
        {
            if (timeout.HasValue)
            {
                var cancellationSource = new CancellationTokenSource();
                cancellationSource.Token.Register(() => cancelToken.TrySetCanceled());
                cancellationSource.CancelAfter(timeout.Value);
            }
        }

        [TestMethod]
        public void MessageSerialization()
        {
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.
            MessageSerializationActor serializer = new MessageSerializationActor();
            SimpleMessage<string> msgToSerialize = new SimpleMessage<string>();
            msgToSerialize.Status = GetMessageCompletionMonitor();

            msgToSerialize.Message = "Foo";

            if (!serializer.OnMessageRecieved(msgToSerialize))
                Console.Error.WriteLine("Serialization Failed.");

            msgToSerialize.Status.Task.Wait();

            byte[] serializedBytes = (msgToSerialize.Status.Task.Result.ProcessingResult as byte[]);

            SerializeMessage msgToSerialize2 = new SerializeMessage(msgToSerialize);
            msgToSerialize.ProcessingResult = null;

            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => new MessageSerializationActor());

            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            byte[] serializedBytes2 = (task.Result.ProcessingResult as byte[]);
        }

        [TestMethod]
        public void MessageDeSerialization()
        {
            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.
            MessageSerializationActor serializer = new MessageSerializationActor();
            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            SimpleMessage<string> msgToSerialize = new SimpleMessage<string>();
            msgToSerialize.Status = GetMessageCompletionMonitor();

            msgToSerialize.Message = "Foo";

            if (!serializer.OnMessageRecieved(msgToSerialize))
                Console.Error.WriteLine("Serialization Failed.");

            msgToSerialize.Status.Task.Wait();

            byte[] serializedBytes = (msgToSerialize.Status.Task.Result.ProcessingResult as byte[]);

            DeSerializeMessage deserializationMessage = new DeSerializeMessage(serializedBytes);

            if (!deserializer.OnMessageRecieved(deserializationMessage))
                Console.Error.WriteLine("De-serialization Failed.");

            string deserilizedMessage = (deserializationMessage.Message as string);

            msgToSerialize.ProcessingResult = null;

            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(() => new MessageDeserializationActor());

            DeSerializeMessage msgToSerialize2 = new DeSerializeMessage(serializedBytes);
            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            string output = (task.Result.Message as string);
        }
    }
}
