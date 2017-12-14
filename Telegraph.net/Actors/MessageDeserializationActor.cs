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

    public class MessageDeserializationActor : IActor
    {
        public virtual bool OnMessageRecieved<T>(T msg) where T : IActorMessage
        {
            if (null == msg.Message)
                throw new NullReferenceException("Could not deserialize " + msg.GetType() + " because the message was null.");

            if (msg.GetType() != typeof(DeSerializeMessage))
                throw new CantDeserlializeAnythingButADeSerializeMessageException();

            if (!(msg.Message is byte[]))
                throw new CantDeserlializeAnythingButADeSerializeMessageException("The Message object in the message was not a byte array so we cannot deserialize it.");

            byte[] toDeSerialize = (byte[])msg.Message;
            ushort version = BitConverter.ToUInt16(toDeSerialize, 0);

            //TODO: check the version with enum

            switch (version)
            {
                case 0:
                    {
                        uint minSize = MessageSerializationActor.MinSizeOfSerializedMessage;

                        if (toDeSerialize.Length < minSize)
                            throw new CantDeserlializeMessageBytesSmallerThanMinSizeException();

                        return OnNonSerlializableMessageRecieved(msg, toDeSerialize);
                    }

                case 1:
                    return OnNonSerlializableMessageRecieved(msg, toDeSerialize);

                default:
                    throw new CantDeserlializeUnknownVersionException("Version:"+version.ToString());

            }//switch
        }

        private bool OnSerializableMsgRecieved<T>(T msg) where T : IActorMessage
        {
            throw new NotImplementedException();
        }

        private bool OnNonSerlializableMessageRecieved<T>(T msg,byte[] toDeSerialize) where T : IActorMessage
        {
            // <verstion-short> (always Zero for this type of serializedMessage)
            // 16byte.message.guid
            // <size.of.type.string.in.Bytes-short>
            // <bytes.of.type.string-bytearray>
            // <size.of.message-uint>
            // <bytes.of.message-bytearray>
            // <size.of.result-uint>
            // <bytes.of.result-bytearray>
            // <status.flags-uint>

            var messageIDBytes = new byte[16];
            byte[] typeByteArray = null;
            byte[] msgByteArray = null;
            byte[] resultByteArray = null;
            uint flags = 0;

            uint serializationIndex = sizeof(ushort); //skip the version number
            byte[] seralizedMsg = (byte[])msg.Message;
            Array.Copy(seralizedMsg, serializationIndex, messageIDBytes, 0, 16);                serializationIndex += (uint)messageIDBytes.Length;
            Guid messageID = new Guid(messageIDBytes);

            uint sizeOfType = BitConverter.ToUInt32(seralizedMsg, (int)serializationIndex);     serializationIndex += sizeof(uint);
            typeByteArray = new byte[sizeOfType];
            Array.Copy(seralizedMsg, serializationIndex, typeByteArray, 0, sizeOfType); serializationIndex += sizeOfType;
            
            uint sizeOfMessage = BitConverter.ToUInt32(seralizedMsg, (int)serializationIndex);  serializationIndex += sizeof(uint);
            msgByteArray = new byte[sizeOfMessage];
            Array.Copy(seralizedMsg, serializationIndex, msgByteArray, 0, sizeOfMessage);       serializationIndex += sizeOfMessage;

            uint sizeOfResult = BitConverter.ToUInt32(seralizedMsg, (int)serializationIndex);   serializationIndex += sizeof(uint);
            resultByteArray = new byte[sizeOfResult];
            Array.Copy(seralizedMsg, serializationIndex, resultByteArray, 0, sizeOfResult);     serializationIndex += sizeOfResult;

            flags = BitConverter.ToUInt32(seralizedMsg, (int)serializationIndex);               serializationIndex += sizeof(uint);

            // I now know the type of the message I need to instantiate it.
            IFormatter formatter = new BinaryFormatter();
            Type messageType = (Type)formatter.Deserialize(new MemoryStream(typeByteArray));

            IActorMessage resultMsg = null;
            if (msg is AnonAskMessage<DeSerializeMessage>)
            {
                (msg as AnonAskMessage<DeSerializeMessage>).OriginalMessage.Type = messageType;
                resultMsg = (msg as AnonAskMessage<DeSerializeMessage>).OriginalMessage;
            }
            else
            {
                (msg as DeSerializeMessage).Type = messageType;
                resultMsg = msg;
            }

            object message = null;
            if (0 != sizeOfMessage)
                message = formatter.Deserialize(new MemoryStream(msgByteArray));

            object result = null;
            if (0 != sizeOfResult)
                result = formatter.Deserialize(new MemoryStream(resultByteArray));

            resultMsg.Message = message;
            resultMsg.ProcessingResult = result;

            if (null != msg.Status)
            {
                msg.Status.SetResult(resultMsg);
            }

            return true;
        }
    }
}
