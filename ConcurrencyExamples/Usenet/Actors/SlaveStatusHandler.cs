using System;
using System.Threading.Tasks;

namespace Usenet.Actors
{
    using Telegraphy.Net;

    class SlaveStatusHandler: SocketBasedActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            if (msg.GetType() != typeof(Messages.RequestSlaveStatus))
                return false;

            ServerConnection conn = (msg as Messages.RequestSlaveStatus).Connection;

            // This command indicates to the server that this client connectino is to a slate server, rather than a user.
            Write(conn, NNTPMessages.Slave);

            string response = Response(conn, NNTPMessages.Slave);

            if (response.Substring(0, 3) != NNTPResponseCodes.SlaveStatusNoted)
            {
                throw new Exception(); //TODO throw new NntpException(response);
            }
            else if (response.ToLower().Contains("unsupported"))
            {
                conn.SupportedCommands[NNTPMessages.Slave] = false;
            }

            return false;
        }
    }
}
