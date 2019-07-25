using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace UnitTests.TestImplemenations
{
    public class TestFirstQuestion : IActorMessage, ISerializable
    {
        private int msgVersion = 1;
        private long orderId = 0;
        private string intervieweeId = string.Empty;

        public long OrderId { get => orderId; set => orderId = value; }
        public string IntervieweeId { get => intervieweeId; set => intervieweeId = value; }

        private const string MSGVERSIONINFO = "vers";
        private const string QUESTIONINFO = "question";
        private const string ORDERIDINFO = "orderid";
        private const string INTERVIEWEEIDINFO = "intervieweeid";

        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }

        public string Question { get { return (string)this.Message; } }

        public TestFirstQuestion(string question, long orderId, string intervieweeId)
        {
            this.Message = question;
            this.orderId = orderId;
            this.intervieweeId = intervieweeId;
        }

        public TestFirstQuestion()
        {
            /* for serialization only */
        }

        protected TestFirstQuestion(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.msgVersion = info.GetInt32(MSGVERSIONINFO);
            this.orderId = info.GetInt64(ORDERIDINFO);
            this.intervieweeId = info.GetString(INTERVIEWEEIDINFO);

            this.Message = info.GetString(QUESTIONINFO);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(MSGVERSIONINFO, this.msgVersion);
            info.AddValue(ORDERIDINFO, this.orderId);
            info.AddValue(INTERVIEWEEIDINFO, this.intervieweeId);
            info.AddValue(QUESTIONINFO, this.Question);
        }
    }
}
