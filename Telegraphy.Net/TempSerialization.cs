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
            byte[] msgBytes = null;
            if (typeof(MsgType) == typeof(string))
            {
                System.Diagnostics.Debug.WriteLine("Serializing " + (string)(msg as IActorMessage).Message);
                if ((msg as IActorMessage).Message.GetType().Name.Equals("String"))
                    msgBytes = Encoding.UTF8.GetBytes((string)(msg as IActorMessage).Message);
                else
                    throw new NotConfiguredToSerializeThisTypeOfMessageException("String");
            }
            else if (typeof(MsgType) == typeof(byte[]))
            {
                if ((msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                    msgBytes = (byte[])(msg as IActorMessage).Message;
                else
                    throw new NotConfiguredToSerializeThisTypeOfMessageException("Byte[]");
            }
            else if (msg is MsgType)
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage<MsgType>((MsgType)msg));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }
            else if ((msg as IActorMessage).Message is MsgType && (msg as IActorMessage).Message is System.Runtime.Serialization.ISerializable)
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage<System.Runtime.Serialization.ISerializable>((System.Runtime.Serialization.ISerializable)(msg as IActorMessage).Message));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }
            else
            {
                var serializeTask = Telegraph.Instance.Ask(new SerializeMessage<IActorMessage>(msg));
                msgBytes = (serializeTask.Result.ProcessingResult as byte[]);
            }

            return msgBytes;
        }
    }
}
