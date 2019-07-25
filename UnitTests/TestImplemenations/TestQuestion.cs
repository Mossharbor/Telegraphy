using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.TestImplemenations
{
    public class TestQuestion : TestFirstQuestion
    {
        public TestQuestion(string question, long orderId, string intervieweeId)
            : base(question, orderId, intervieweeId)
        {
        }

        public TestQuestion()
        {
            /* for serialization only */
        }
    }
}
