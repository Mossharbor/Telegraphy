using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace UnitTests.TestImplemenations
{
    public class TestQuestionListExtractor : IActor
    {
        public static int msgRecievedCount = 0;
        public static int msgSentCount = 0;

        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            if (!(msg is TestQuestionListMessage))
                throw new NotImplementedException("Question Extractor does not support processing msg type:" + msg.GetType());
            
            TestQuestionListMessage list = msg as TestQuestionListMessage;
            ++msgRecievedCount;

            ++msgSentCount;
            Telegraph.Instance.Ask(new TestFirstQuestion(list.Questions[0], list.OrderId, list.IntervieweeId)).Wait();

            for (int i = 1; i < list.Questions.Length; ++i)
            {
                ++msgSentCount;
                Telegraph.Instance.Ask(new TestQuestion(list.Questions[i], list.OrderId, list.IntervieweeId)).Wait();
            }

            return true;
        }
    }
}
