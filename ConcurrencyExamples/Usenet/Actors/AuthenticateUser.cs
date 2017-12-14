using System;

namespace Usenet.Actors
{
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using Telegraphy.Net;

    class AuthenticateUser : SocketBasedActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            if (msg.GetType() != typeof(Messages.Authenticate))
                return false;

            string authResponse = "";
            var conn = (msg as Messages.Authenticate).Connection;
            TcpClient connection = conn.Connection;
            NetworkStream stream = conn.Stream;
            string Username = conn.UserName;
            string Password = conn.Password;
            System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding();

            if ((null == Username) || (null == Password))
                return false;

            //DO Authetication Here!
            bool success = true;
            string message = NNTPMessages.GetAuthInfoUser(Username);
            var writeBuffer = en.GetBytes(message);
            stream.Write(writeBuffer, 0, writeBuffer.Length);

            string requiresPassword = Response(conn, message);

            //TODO: check the supported commands for the correct command to authenticate with.
            authResponse = requiresPassword;

            if (NNTPResponseCodes.IsResposeCode(requiresPassword, NNTPResponseCodes.PasswordRequired))
            {
                message = NNTPMessages.GetAuthInfoPass(Password);
                writeBuffer = en.GetBytes(message);
                stream.Write(writeBuffer, 0, writeBuffer.Length);

                string response = Response(conn, message);

                if (NNTPResponseCodes.IsResposeCode(response, NNTPResponseCodes.AuthenticationOK))
                    success = true;

                if (NNTPResponseCodes.IsResposeCode(response, NNTPResponseCodes.PermissionDenied))
                    throw new Exception(response); //NNTPAuthenticationDeniedException
            }  

            return true;
        }
    }
}
