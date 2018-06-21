using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Net
{
    public class TempSerialization
    {
        public static byte[] GetBytes<MsgType>(IActorMessage msg)
        {
            byte[] msgBytes = GetBytesFromType(typeof(MsgType), msg);

            if (null != msgBytes)
                return msgBytes;

            if (msg is MsgType)
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage<MsgType>((MsgType)msg));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }

            else
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage<IActorMessage>(msg));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }
            return msgBytes;
        }

        private static byte[] GetBytesFromType(Type MsgType, IActorMessage msg)
        {
            byte[] msgBytes = null;
            if (MsgType == typeof(string))
            {
                System.Diagnostics.Debug.WriteLine("Serializing " + (string)(msg as IActorMessage).Message);
                if ((msg as IActorMessage).Message.GetType().Name.Equals("String"))
                    msgBytes = Encoding.UTF8.GetBytes((string)(msg as IActorMessage).Message);
                else
                    throw new NotConfiguredToSerializeThisTypeOfMessageException("String");
            }
            else if (MsgType == typeof(byte[]))
            {
                if ((msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                    msgBytes = (byte[])(msg as IActorMessage).Message;
                else
                    throw new NotConfiguredToSerializeThisTypeOfMessageException("Byte[]");
            }
            else if (MsgType == msg.GetType())
            {
                return null;
            }
            else if (MsgType == (msg as IActorMessage).Message.GetType() && (msg as IActorMessage).Message is System.Runtime.Serialization.ISerializable)
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage<System.Runtime.Serialization.ISerializable>((System.Runtime.Serialization.ISerializable)(msg as IActorMessage).Message));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }

            return msgBytes;
        }

        public static IActorMessage GetTypeFromBytes<MsgType>(byte[] bytes) where MsgType:class, IActorMessage
        {
            var serializeTask = Telegraph.Instance.Ask(new DeserializeMessage<MsgType>(bytes));
            return (serializeTask.Result as MsgType);
        }
    }
}
