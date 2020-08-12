using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Telegraphy.Net
{
    class MessageTypeFormatter
        : IFormatter
    {
        int bufferSize = 1024;

        public MessageTypeFormatter()
        {
        }

        public SerializationBinder Binder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public StreamingContext Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISurrogateSelector SurrogateSelector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object Deserialize(Stream serializationStream)
        {
            List<byte> allBytes = new List<byte>();
            byte[] buffer = new Byte[this.bufferSize];

            while (0 != serializationStream.Read(buffer, 0, this.bufferSize))
            {
                allBytes.AddRange(buffer);
            }

            return Encoding.UTF8.GetString(allBytes.ToArray());
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            var bytes = Encoding.UTF8.GetBytes(graph.ToString());

            serializationStream.Write(bytes, 0, bytes.Length);
        }
    }
}
