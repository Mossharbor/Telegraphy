using System;

namespace Usenet.Actors
{
    using System.Net.Sockets;
    using Telegraphy.Net;

    class ResponseHandler : DefaultActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            if (msg.GetType() != typeof(Messages.GetReponse))
                throw new NotImplementedException();

            ServerConnection conn = (msg as Messages.GetReponse).Connection;

            NetworkStream stream = conn.Stream;
            TcpClient connection = conn.Connection;
            string _lastMessage = (msg as Messages.GetReponse).LastSentMessage;
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string lastResponseCode = "";
            
            int count = 0;
            int bytes = 0;
            bool messageGreaterThanBuffer = false;
            byte[] buff = new Byte[2];
            byte[] serverbuff = new Byte[2048];
            System.Diagnostics.Debug.WriteLine("Waiting for response:" + conn.ServerName);

            do
            {
                int attempts;
                do
                {

                    try
                    {
                        SocketError error;
                        attempts = 100;
                        while ((bytes = connection.Client.Receive(buff,0,1, SocketFlags.Peek, out error)) == 0
                            && 0 < attempts)
                        {
                            --attempts;
                            //LibrayMessageLogger.Instance.WriteError(this, NNTPErrorTypes.InvalidResponse, error.ToString() + " socket error.");

                            if (!connection.Connected)
                                System.Diagnostics.Debug.WriteLine("Error not connected to "+conn.ServerName);

                            System.Threading.Thread.Sleep(100);
                        }

                        bytes = stream.Read(buff, 0, 1);
                        if (bytes == 1)
                        {
                            serverbuff[count] = buff[0];
                            ++count;
                        }
                    }
                    catch (Exception e)
                    {
                        int i = 0;
                        throw;
                    }

                } while ((bytes == 1) && (buff[0] != '\n') && (count < serverbuff.Length - 1));

                if (!messageGreaterThanBuffer)
                    lastResponseCode = enc.GetString(serverbuff, 0, count);
                else
                    lastResponseCode += enc.GetString(serverbuff, 0, count);

                if (count >= serverbuff.Length)
                    messageGreaterThanBuffer = true;
                else
                    messageGreaterThanBuffer = false;

                //See if we need to authenticate
                if ((conn.Authenticate) && (3 <= lastResponseCode.Length) && (lastResponseCode.Substring(0, 3) == NNTPResponseCodes.AuthenticationRequired))
                {
                    if (!AuthenticateUser(conn))
                        throw new Exception(); //NntpException("Error could not authenticate user for server " + +" AuthString:" + authenticateString + " LastResponse:" + lastResponseCode);

                    if (String.IsNullOrEmpty(_lastMessage))
                        break;

                    var writeBuffer = enc.GetBytes(_lastMessage);
                    stream.Write(writeBuffer, 0, writeBuffer.Length);

                    return this.OnMessageRecieved(msg);
                }

            } while (messageGreaterThanBuffer);

            msg.ProcessingResult = lastResponseCode;

            return false;
        }

        public bool AuthenticateUser(ServerConnection conn)
        {
            var t = Telegraph.Instance.Ask(new Messages.Authenticate(conn));

            t.Wait();

            return true;
        }
    }
}
