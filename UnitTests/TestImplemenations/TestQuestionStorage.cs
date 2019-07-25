using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace UnitTests.TestImplemenations
{
    public interface IQuestionStorage : IActor
    {
    }

    public class TestQuestionStorage : IQuestionStorage
    {
        public int msgRecievedCount = 0;

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            ++msgRecievedCount;
            return true;
        }
    }
}
