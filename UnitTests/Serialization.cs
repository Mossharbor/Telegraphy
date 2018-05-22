using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Serialization
{
    using Telegraphy.Net;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    public class Serialization
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
            IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
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
            IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
            SimpleMessage<string> msgToSerialize = new SimpleMessage<string>();
            msgToSerialize.Message = "Foo";

            SerializeIActorMessage serializeMessage = new SerializeIActorMessage(msgToSerialize);
            msgToSerialize.ProcessingResult = null;

            Telegraph.Instance.Register<SerializeIActorMessage, IActorMessageSerializationActor>(() => new IActorMessageSerializationActor());

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
            IActorMessageSerializationActor serializer = new IActorMessageSerializationActor();
            IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
            SimpleMessage<string> msgToSerialize = new SimpleMessage<string>();
            msgToSerialize.Status = GetMessageCompletionMonitor();

            msgToSerialize.Message = "Foo";

            if (!serializer.OnMessageRecieved(msgToSerialize))
                Console.Error.WriteLine("Serialization Failed.");

            msgToSerialize.Status.Task.Wait();

            byte[] serializedBytes = (msgToSerialize.Status.Task.Result.ProcessingResult as byte[]);

            DeserializeIActorMessage deserializationMessage = new DeserializeIActorMessage(serializedBytes);

            if (!deserializer.OnMessageRecieved(deserializationMessage))
                Console.Error.WriteLine("De-serialization Failed.");

            string deserilizedMessage = (deserializationMessage.Message as string);

            msgToSerialize.ProcessingResult = null;

            Telegraph.Instance.Register<DeserializeIActorMessage, IActorMessageDeserializationActor>(() => new IActorMessageDeserializationActor());

            DeserializeIActorMessage msgToSerialize2 = new DeserializeIActorMessage(serializedBytes);
            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            string output = (task.Result.Message as string);
        }

        [TestMethod]
        public void DeSerializeInDeserializeClass()
        {
            byte[] serializedBytes = SimpleMessageSerializationReturn();
            IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
            DeserializeIActorMessage deserializationMessage = new DeserializeIActorMessage(serializedBytes);

            if (!deserializer.OnMessageRecieved(deserializationMessage))
                Assert.Fail("De-serialization Failed.");

            Assert.IsTrue(deserializationMessage.Message.Equals("Foo"));
        }

        [TestMethod]
        public void DeSerializeDeSerializeMessage()
        {
            byte[] serializedBytes = SimpleMessageSerializationReturn();
            IActorMessageDeserializationActor deserializer = new IActorMessageDeserializationActor();
            DeserializeIActorMessage deserializationMessage = new DeserializeIActorMessage(serializedBytes);
            Telegraph.Instance.Register<DeserializeIActorMessage, IActorMessageDeserializationActor>(() => new IActorMessageDeserializationActor());

            DeserializeIActorMessage msgToSerialize2 = new DeserializeIActorMessage(serializedBytes);

            var task = Telegraph.Instance.Ask(msgToSerialize2);

            task.Wait();

            string output = (task.Result.Message as string);
            Assert.IsTrue(output.Equals("Foo"));

        }

        [TestMethod]
        public void SerializeAValueTypeMessage()
        {
            IActorMessageSerializationActor serialization = new IActorMessageSerializationActor();
            IActorMessageDeserializationActor deserialization = new IActorMessageDeserializationActor();
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
            TestValTypeArray<int>(serialization, deserialization, new int[] { 4, 3 });
            TestValType<int>(serialization, deserialization, 4);
            TestValType<double>(serialization, deserialization, 4.2);
            TestValTypeArray<double>(serialization, deserialization, new double[] { 4, 3 });
            TestValType<float>(serialization, deserialization, 4.1F);
            TestValTypeArray<float>(serialization, deserialization, new float[] { 4, 3 });
            TestValType<byte>(serialization, deserialization, 8);
            TestValTypeArray<byte>(serialization, deserialization, new byte[] { 4, 3 });
            TestValType<sbyte>(serialization, deserialization, 2);
            TestValTypeArray<sbyte>(serialization, deserialization, new sbyte[] { 4, 3 });
            TestValType<short>(serialization, deserialization, 30);
            TestValTypeArray<short>(serialization, deserialization, new short[] { 4, 3 });
            TestValType<ushort>(serialization, deserialization, 200);
            TestValTypeArray<ushort>(serialization, deserialization, new ushort[] { 4, 3 });
            TestValType<uint>(serialization, deserialization, uint.MaxValue);
            TestValTypeArray<uint>(serialization, deserialization, new uint[] { 4, 3 });
            TestValType<long>(serialization, deserialization, long.MinValue);
            TestValTypeArray<long>(serialization, deserialization, new long[] { 4, 3 });
            TestValType<ulong>(serialization, deserialization, ulong.MaxValue);
            TestValTypeArray<ulong>(serialization, deserialization, new ulong[] { 4, 3 });
            TestValType<ulong>(serialization, deserialization, ulong.MinValue);
            TestValTypeArray<ulong>(serialization, deserialization, new ulong[] { 4, 3 });
            TestValType<bool>(serialization, deserialization, true);
            TestValTypeArray<bool>(serialization, deserialization, new bool[] { true, false });
            TestValType<bool>(serialization, deserialization, false);
            TestValType<decimal>(serialization, deserialization, (decimal)5.67);
            TestValTypeArray<decimal>(serialization, deserialization, new decimal[] { 4, 3 });
            TestValType<char>(serialization, deserialization, 'a');
            TestValTypeArray<char>(serialization, deserialization, new char[] { 'a', 'c' });
            TestValType<DateTime>(serialization, deserialization, DateTime.Now);
            TestValType<TimeSpan>(serialization, deserialization, DateTime.Now - start);
            TestValType<Guid>(serialization, deserialization, Guid.NewGuid());
            TestValTypeArray<Guid>(serialization, deserialization, new Guid[] { Guid.NewGuid(), Guid.NewGuid() });
            TestValType<DateTimeOffset>(serialization, deserialization,new DateTimeOffset(start));
            var myDt = DateTime.SpecifyKind(new DateTime(1,1,1), DateTimeKind.Unspecified);
            TestValType<TimeZoneInfo.TransitionTime>(serialization, deserialization, TimeZoneInfo.TransitionTime.CreateFixedDateRule(myDt, 10,11));
        }

        private static void TestValType<T>(IActorMessageSerializationActor serialization, IActorMessageDeserializationActor deserialization, T val) where T : IEquatable<T>
        {
            ValueTypeMessage<T> intType = new ValueTypeMessage<T>(val);
            var typeVal = (T)intType.Message;
            string id = intType.Id;

            SerializeIActorMessage sMsg = new SerializeIActorMessage(intType);
            if (!serialization.OnMessageRecieved(sMsg))
                Assert.Fail(string.Format("Seriaization of {0} Failed", val.GetType().Name));

            DeserializeIActorMessage dMsg = new DeserializeIActorMessage(sMsg.ProcessingResult as byte[]);
            if (!deserialization.OnMessageRecieved(dMsg))
                Assert.Fail(string.Format("DeSeriaization of {0} Failed", val.GetType().Name));

            Assert.IsTrue((dMsg.ProcessingResult as ValueTypeMessage<T>).Message.Equals(typeVal));
            Assert.IsTrue((dMsg.ProcessingResult as IActorMessageIdentifier).Id.Equals(id));
        }

        private static void TestValTypeArray<T>(IActorMessageSerializationActor serialization, IActorMessageDeserializationActor deserialization, T[] val) where T : IEquatable<T>
        {
            ValueTypeMessage<T> intType = new ValueTypeMessage<T>(val);
            var typeVal = (T[])intType.Message;
            string id = intType.Id;

            SerializeIActorMessage sMsg = new SerializeIActorMessage(intType);
            if (!serialization.OnMessageRecieved(sMsg))
                Assert.Fail(string.Format("Seriaization of array {0} Failed", val.GetType().Name));

            DeserializeIActorMessage dMsg = new DeserializeIActorMessage(sMsg.ProcessingResult as byte[]);
            if (!deserialization.OnMessageRecieved(dMsg))
                Assert.Fail(string.Format("DeSeriaization of array {0} Failed", val.GetType().Name));

            T[] message = (T[])(dMsg.ProcessingResult as ValueTypeMessage<T>).Message;

            for(int i=0; i < message.Length; ++i)
            {
                Assert.IsTrue(message[i].Equals(typeVal[i]));
            }
            Assert.IsTrue(message.Length != 0);
            Assert.IsTrue((dMsg.ProcessingResult as IActorMessageIdentifier).Id.Equals(id));
        }
    }
}
