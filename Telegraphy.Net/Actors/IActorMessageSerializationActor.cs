using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    public class IActorMessageSerializationActor : IActor
    {
        internal static readonly uint MinSizeOfSerializedMessage = 2+16+4+4+4+4;//minimum size without message or result

        public virtual bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            if (null == msg.Message)
                throw new NullReferenceException("Could not serialize " + msg.GetType() + " because the message was null.");

            IActorMessage toSerialize = null;
            if (msg is AnonAskMessage<SerializeMessage<IActorMessage>>)
            {
                toSerialize = (msg as AnonAskMessage<SerializeMessage<IActorMessage>>).OriginalMessage.MessageToSerialize;
            }
            else if (msg is SerializeMessage<IActorMessage>)
            {
                toSerialize = (msg as SerializeMessage<IActorMessage>).MessageToSerialize;
            }
            else
            {
                toSerialize = new SerializeMessage<IActorMessage>(msg).MessageToSerialize;
            }


            IFormatter formatter = new BinaryFormatter();
            byte[] messageInBytes = null;
            byte[] resultInBytes = null;
            ushort version = ushort.MaxValue;

            if (toSerialize is ISerializable)
            {
                version = 1; //TODO enum
                MemoryStream ms = new MemoryStream();
                formatter.Serialize(ms, toSerialize);
                messageInBytes = ms.GetBuffer();
                ms = null;
                resultInBytes = new byte[0];
            }
            else
            {
                version = 0; //TODO enum
                if (null != toSerialize.Message)
                {
                    MemoryStream ms = new MemoryStream();
                    formatter.Serialize(ms, toSerialize.Message);
                    messageInBytes = ms.GetBuffer();
                    ms = null;
                }
                else
                {
                    messageInBytes = new byte[0];
                }

                if (null != toSerialize.ProcessingResult)
                {
                    MemoryStream ms = new MemoryStream();
                    formatter.Serialize(ms, toSerialize.ProcessingResult);
                    resultInBytes = ms.GetBuffer();
                    ms = null;
                }
                else
                {
                    resultInBytes = new byte[0];
                }
            }
            
            return Serialize(msg, toSerialize, version, messageInBytes, resultInBytes);
        }

        private bool Serialize<T>(T msg, IActorMessage toSerialize,ushort version, byte[] messageBytes, byte[] resultBytes) where T : IActorMessage
        {
            // <verstion-short> (always Zero for this type of serializedMessage)
            // 16byte.message.guid
            // <size.of.type.in.Bytes-ushort>
            // <bytes.of.type.string-bytearray>
            // <size.of.message-uint>
            // <bytes.of.message-bytearray>
            // <size.of.result-uint>
            // <bytes.of.result-bytearray>
            // <status.flags-uint>

            uint serializedByteCount = IActorMessageSerializationActor.MinSizeOfSerializedMessage; 

            var messageID = Guid.NewGuid().ToByteArray();

            byte[] typeBytes = null;
            uint sizeOfType = 0;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream typeMs = new MemoryStream();
            formatter.Serialize(typeMs, toSerialize);
            typeBytes = typeMs.GetBuffer();
            typeMs = null;
            sizeOfType = (uint)typeBytes.Length;

            uint statusFlags = 0; //TODO:

            byte[] serializedMsg = CreateSerializationPackage(version, typeBytes, messageID, messageBytes, resultBytes, statusFlags);

            if (null != msg.Status)
            {
                if (!(msg.GetType() == typeof(SerializeMessage<IActorMessage>)))
                {
                    msg.Status.SetResult(new SerializeMessage<IActorMessage>(toSerialize, serializedMsg));
                }
                else
                {
                    msg.ProcessingResult = serializedMsg;
                    msg.Status.SetResult(msg);
                }
            }
            else
                msg.ProcessingResult = serializedMsg;

            return true;
        }

        private byte[] CreateSerializationPackage(
            ushort version,
            byte[] typeBytes,
            byte[] messageID,
            byte[] messageInBytes,
            byte[] resultInBytes,
            uint statusFlags)
        {
            var sizeOfResult= resultInBytes.Length;
            var sizeOfMessage = messageInBytes.Length;
            var sizeOfType = typeBytes.Length;
            uint serializedByteCount = IActorMessageSerializationActor.MinSizeOfSerializedMessage;
            serializedByteCount += (uint)(sizeOfResult + sizeOfMessage + sizeOfType);

            byte[] serializedMsg = new byte[serializedByteCount];

            uint copyToIndex = 0;
            Array.Copy(BitConverter.GetBytes((ushort)version), 0, serializedMsg, copyToIndex, sizeof(ushort));  copyToIndex += sizeof(ushort);
            Array.Copy(messageID, 0, serializedMsg, copyToIndex, messageID.Length);                             copyToIndex += (uint)messageID.Length;
            Array.Copy(BitConverter.GetBytes(sizeOfType), 0, serializedMsg, copyToIndex, sizeof(uint));         copyToIndex += sizeof(uint);
            Array.Copy(typeBytes, 0, serializedMsg, copyToIndex, typeBytes.Length);                             copyToIndex += (uint)typeBytes.Length;
            Array.Copy(BitConverter.GetBytes(sizeOfMessage), 0, serializedMsg, copyToIndex, sizeof(uint));      copyToIndex += sizeof(uint);
            Array.Copy(messageInBytes, 0, serializedMsg, copyToIndex, messageInBytes.Length);                   copyToIndex += (uint)messageInBytes.Length;
            Array.Copy(BitConverter.GetBytes(sizeOfResult), 0, serializedMsg, copyToIndex, sizeof(uint));       copyToIndex += sizeof(uint);
            Array.Copy(resultInBytes, 0, serializedMsg, copyToIndex, resultInBytes.Length);                     copyToIndex += (uint)resultInBytes.Length;
            Array.Copy(BitConverter.GetBytes(statusFlags), 0, serializedMsg, copyToIndex, sizeof(uint));        copyToIndex += sizeof(uint);

            return serializedMsg;
        }
    }
}
