using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartHere
{
    using Telegraphy.Net;
    using Telegraphy.Msmq;

    class Program
    {
        static void Main(string[] args)
        {
            // Turn msmq on on windows 10
            //     Open Control Panel.
            //     Click Programs and then, under Programs and Features, click Turn Windows Features on and off.
            //     Expand Microsoft Message Queue(MSMQ) Server, expand Microsoft Message Queue(MSMQ) Server Core, and then select the check boxes for the following Message Queuing features to install. ...
            //     Click OK.

            // create message
            string message = @"Hello World";
            string queueName = "testQueue";

            // Setup send to queue with an actor that uses lazy instatiation
            Telegraph.Instance.Register<byte[], MsmqDeliveryOperator<byte[]>>(() => new MsmqDeliveryOperator<byte[]>(queueName));

            // Send message to queue
            List<Task> waitTasks = new List<Task>();
            int i = 0;
            for (; i < 100; ++i)
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                waitTasks.Add(Telegraph.Instance.Ask(msgBytes));
            }

            Console.WriteLine("Messages sent");
            Task.WaitAll(waitTasks.ToArray());
            Telegraph.Instance.UnRegisterAll();

            int count = 0;
            long msmqOperatorID = Telegraph.Instance.Register(new MsmqReceptionOperator<byte[]>(queueName));
            Telegraph.Instance.Register<byte[]>(msmqOperatorID, bytemsg =>
            {
                string stringmsg = Encoding.UTF8.GetString(bytemsg);
                System.Threading.Thread.Sleep(100);
                System.Threading.Interlocked.Increment(ref count); 
                Console.WriteLine(stringmsg);
            });

            int attempts = 20;
            while (attempts != 0 && count < i)
            {
                System.Threading.Thread.Sleep(1000);
                --attempts;
            }

            Console.WriteLine("Recieved:" + count + " Sent:" + MsmqReceptionOperator<byte[]>.TotalSendsCalled);
            Console.ReadLine();
        }
    }
}
