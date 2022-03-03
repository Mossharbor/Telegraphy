using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPong
{
    using System.Runtime.Serialization;
    using Telegraphy.Net;
    using Telegraphy.Azure;

    [Serializable]
    public class Pung : IActorMessage, ISerializable, IServiceBusMessagePropertiesProvider
    {
        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public Pung(string payload,DateTime enqueueTime)
        {
            this.Message = payload;
            this.ScheduledEnqueueTimeUtc = enqueueTime;
            this.MessageId = Guid.NewGuid().ToString(); // this always needs to be set.
        }

        public Pung(string payload)
        {
            this.Message = payload;
            this.MessageId = Guid.NewGuid().ToString(); // this always needs to be set.
        }

        public Pung() : this("Pung") { }

        protected Pung(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.Message = info.GetString("msg");
            this.MessageId = info.GetString("msgid");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("msg", this.Message);
            info.AddValue("msgid", this.MessageId);
        }

        #region IServiceBusPropertiesProvider
        public DateTime? ScheduledEnqueueTimeUtc { get; private set; }

        public string ContentType => null;

        public string Label => null;

        public string CorrelationId => null;

        public TimeSpan? TimeToLive => null;

        public string ReplyToSessionId => null;

        public string SessionId => null;

        public string MessageId { get; private set; }

        public IDictionary<string, object> ApplicationProperties => null;

        public string PartitionKey => null;
        #endregion
    }
}
