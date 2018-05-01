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
        public void SimpleMessageSerialization()
        {
            SimpleMessageSerializationReturn();
        }

        public byte[] SimpleMessageSerializationReturn()
        {
            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.
            MessageSerializationActor serializer = new MessageSerializationActor();
            SimpleMessage<string> msgToSerialize = new SimpleMessage<string>();
            msgToSerialize.Status = GetMessageCompletionMonitor();

            msgToSerialize.Message = "Foo";

            if (!serializer.OnMessageRecieved(msgToSerialize))
                Console.Error.WriteLine("Serialization Failed.");

            msgToSerialize.Status.Task.Wait();

            byte[] serializedBytes = (msgToSerialize.Status.Task.Result.ProcessingResult as byte[]);

            return serializedBytes;

        }

        [TestMethod]
        public void SerializeSerializeMessage()
        {
            SerializeSerializeMessageReturn();
        }

        public byte[] SerializeSerializeMessageReturn()
        {
            Telegraph.Instance.UnRegisterAll();

            Telegraph.Instance.MainOperator = new LocalOperator(new LocalSwitchboard(LocalConcurrencyType.ActorsOnThreadPool)); // performs a reset.
            MessageSerializationActor serializer = new MessageSerializationActor();
            SimpleMessage<string> msgToSerialize = new SimpleMessage<string>();
            msgToSerialize.Message = "Foo";

            SerializeMessage serializeMessage = new SerializeMessage(msgToSerialize);
            msgToSerialize.ProcessingResult = null;

            Telegraph.Instance.Register<SerializeMessage, MessageSerializationActor>(() => new MessageSerializationActor());

            var task = Telegraph.Instance.Ask(serializeMessage);

            task.Wait();

            byte[] serializedBytes2 = (task.Result.ProcessingResult as byte[]);

            return serializedBytes2;
        }

        [TestMethod]
        public void SimpleMessageDeSerialization()
        {
            Telegraph.Instance.UnRegisterAll();

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

        [TestMethod]
        public void DeSerializeInDeserializeClass()
        {
            byte[] serializedBytes = SimpleMessageSerializationReturn();
            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            DeSerializeMessage deserializationMessage = new DeSerializeMessage(serializedBytes);

            if (!deserializer.OnMessageRecieved(deserializationMessage))
                Assert.Fail("De-serialization Failed.");

            Assert.IsTrue(deserializationMessage.Message.Equals("Foo"));
        }

        [TestMethod]
        public void DeSerializeDeSerializeMessage()
        {
            byte[] serializedBytes = SimpleMessageSerializationReturn();
            MessageDeserializationActor deserializer = new MessageDeserializationActor();
            DeSerializeMessage deserializationMessage = new DeSerializeMessage(serializedBytes);
            Telegraph.Instance.Register<DeSerializeMessage, MessageDeserializationActor>(() => new MessageDeserializationActor());

            DeSerializeMessage msgToSerialize2 = new DeSerializeMessage(serializedBytes);

            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            string output = (task.Result.Message as string);
            Assert.IsTrue(output.Equals("Foo"));

        }

        [TestMethod]
        public void SerializeAValueTypeMessage()
        {
            MessageSerializationActor serialization = new MessageSerializationActor();
            MessageDeserializationActor deserialization = new MessageDeserializationActor();
            deserialization.Register((object msg) => (ValueTypeMessage<int>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<double>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<float>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<byte>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<sbyte>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<short>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<ushort>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<uint>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<double>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<long>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<ulong>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<bool>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<decimal>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<char>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<DateTime>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<TimeSpan>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<Guid>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<DateTimeOffset>)msg);
            //deserialization.Register((object msg) => (ValueTypeMessage<ArraySegment<byte>>)msg);
            deserialization.Register((object msg) => (ValueTypeMessage<TimeZoneInfo.TransitionTime>)msg);

            DateTime start = DateTime.Now;
            DateTime utcStart = start.ToUniversalTime();
            TestValType<int>(serialization, deserialization, 4);
            TestValType<double>(serialization, deserialization, 4.2);
            TestValType<float>(serialization, deserialization, 4.1F);
            TestValType<byte>(serialization, deserialization, 8);
            TestValType<sbyte>(serialization, deserialization, 2);
            TestValType<short>(serialization, deserialization, 30);
            TestValType<ushort>(serialization, deserialization, 200);
            TestValType<uint>(serialization, deserialization, uint.MaxValue);
            TestValType<long>(serialization, deserialization, long.MinValue);
            TestValType<ulong>(serialization, deserialization, ulong.MaxValue);
            TestValType<ulong>(serialization, deserialization, ulong.MinValue);
            TestValType<bool>(serialization, deserialization, true);
            TestValType<bool>(serialization, deserialization, false);
            TestValType<decimal>(serialization, deserialization, (decimal)5.67);
            TestValType<char>(serialization, deserialization, 'a');
            TestValType<DateTime>(serialization, deserialization, DateTime.Now);
            TestValType<TimeSpan>(serialization, deserialization, DateTime.Now - start);
            TestValType<Guid>(serialization, deserialization, Guid.NewGuid());
            TestValType<DateTimeOffset>(serialization, deserialization,new DateTimeOffset(start));
            var myDt = DateTime.SpecifyKind(new DateTime(1,1,1), DateTimeKind.Unspecified);
            TestValType<TimeZoneInfo.TransitionTime>(serialization, deserialization, TimeZoneInfo.TransitionTime.CreateFixedDateRule(myDt, 10,11));
        }

        private static void TestValType<T>(MessageSerializationActor serialization, MessageDeserializationActor deserialization, T val) where T : IEquatable<T>
        {
            ValueTypeMessage<T> intType = new ValueTypeMessage<T>(val);
            var typeVal = (T)intType.Message;
            string id = intType.Id;

            SerializeMessage sMsg = new SerializeMessage(intType);
            if (!serialization.OnMessageRecieved(sMsg))
                Assert.Fail(string.Format("Seriaization of {0} Failed", val.GetType().Name));

            DeSerializeMessage dMsg = new DeSerializeMessage(sMsg.ProcessingResult as byte[]);
            if (!deserialization.OnMessageRecieved(dMsg))
                Assert.Fail(string.Format("DeSeriaization of {0} Failed", val.GetType().Name));

            Assert.IsTrue((dMsg.ProcessingResult as ValueTypeMessage<T>).Message.Equals(typeVal));
            Assert.IsTrue((dMsg.ProcessingResult as IActorMessageIdentifier).Id.Equals(id));
        }
    }
}
