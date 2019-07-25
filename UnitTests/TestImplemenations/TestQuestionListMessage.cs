using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace UnitTests.TestImplemenations
{
    public class TestQuestionListMessage : IActorMessage, ISerializable
    {
        private int msgVersion = 1;
        private long orderId = 0;
        private string intervieweeId = string.Empty;

        public long OrderId { get => orderId; set => orderId = value; }
        public string IntervieweeId { get => intervieweeId; set => intervieweeId = value; }

        private const string MSGVERSIONINFO = "vers";
        private const string QUESTIONINFO = "question";
        private const string QUESTIONCOUNTINFO = "questioncount";
        private const string ORDERIDINFO = "orderid";
        private const string INTERVIEWEEIDINFO = "intervieweeid";

        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }


        public string[] Questions { get { return (string[])this.Message; } }

        public TestQuestionListMessage(string[] questions, long orderId, string intervieweeId)
        {
            this.Message = questions;
            this.OrderId = orderId;
            this.IntervieweeId = intervieweeId;
        }

        public TestQuestionListMessage()
        {
            /* for serialization only */
        }

        protected TestQuestionListMessage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.msgVersion = info.GetInt32(MSGVERSIONINFO);
            this.OrderId = info.GetInt64(ORDERIDINFO);
            this.IntervieweeId = info.GetString(INTERVIEWEEIDINFO);

            int questCount = info.GetInt32(QUESTIONCOUNTINFO);
            string[] questions = new string[questCount];
            for (int i = 0; i < questCount; ++i)
            {
                questions[i] = info.GetString(QUESTIONINFO + i.ToString());
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int questionCount = ((string[])this.Message).Length;
            for (int i = 0; i < questionCount; ++i)
            {
                info.AddValue(QUESTIONINFO + i.ToString(), ((string[])this.Message)[i]);
            }

            info.AddValue(QUESTIONCOUNTINFO, this.Questions.Length);
            info.AddValue(MSGVERSIONINFO, this.msgVersion);
            info.AddValue(ORDERIDINFO, this.OrderId);
            info.AddValue(INTERVIEWEEIDINFO, this.IntervieweeId);
        }
    }
}
