using System;
using System.Threading.Tasks;

namespace Usenet.Actors
{
    using System.Net.Sockets;
    using Telegraphy.Net;

    public abstract class SocketBasedActor : DefaultActor
    {
        System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding();

        protected string Response(ServerConnection conn, string lastMessage)
        {
            var task = Telegraph.Instance.Ask(new Messages.GetReponse(conn, lastMessage));

            task.Wait();

            return (task.Result.ProcessingResult as string);
        }

        protected void Write(ServerConnection conn, string message)
        {
            NetworkStream stream = conn.Stream;
            var writeBuffer = en.GetBytes(message);
            stream.Write(writeBuffer, 0, writeBuffer.Length);
        }

        protected void VerifyResponse(string response)
        {
            if ((null == response) || (3 > response.Length))
            {
                throw new Exception(); //TODO throw new NntpException(response);
            }
        }

        protected void LogResponse(string response)
        {
            Telegraph.Instance.Tell(response);
        }
    }
}
