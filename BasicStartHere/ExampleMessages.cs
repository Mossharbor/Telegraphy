using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicStartHere
{
    using Telegraphy.Net;
    using System.Runtime.Serialization;

    public class Compute : IActorMessage
    {
        private int value = 0;
        public Compute(int i)
        {
            this.Message = this;
            value = i;
        }

        public object Message { get; set; }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public void RaiseToPower(int power)
        {
            this.ProcessingResult = (object)Math.Pow((int)value, power);
        }
    }

    [Serializable]
    public class CustomSerializableMessage : IActorMessage, ISerializable
    {
        public CustomSerializableMessage(int propertyToSerialize, string message)
        {
            this.MyProperty = propertyToSerialize;
            this.Message = message;
        }

        // The special constructor is used to deserialize values. 
        public CustomSerializableMessage(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            MyProperty = info.GetInt32("props");
            Message = info.GetString("message_props");
        }

        public int MyProperty { get; set; }

        public object Message { get; set; }

        public object ProcessingResult { get; set; }

        public TaskCompletionSource<IActorMessage> Status { get; set; }

        // Implement this method to serialize data. The method is called  
        // on serialization. 
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Use the AddValue method to specify serialized values.
            info.AddValue("props", MyProperty);
            info.AddValue("message_props", Message, typeof(string));
        }
    }
}
