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
            for (int i = 0; i < 100; ++i)
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                Telegraph.Instance.Ask(msgBytes).Wait();
            }

            Telegraph.Instance.UnRegisterAll();

            long azureOperatorID = Telegraph.Instance.Register(new MsmqReceptionOperator<byte[]>(queueName));
            Telegraph.Instance.Register<byte[]>(azureOperatorID, bytemsg =>
            {
                string stringmsg = Encoding.UTF8.GetString(bytemsg);
                System.Threading.Thread.Sleep(100);
                Console.WriteLine(stringmsg);
            });

            Console.ReadLine();
        }
    }
}
