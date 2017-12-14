using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet.Actors
{
    class PostMessageHandler :SocketBasedActor
    {
        public override bool OnMessageRecieved<T>(T msg)
        {
            /// The Post method sends a POST message to the NNTP server. 
            /// The POST message takes the newsgroup names as its only parameter. 
            /// The NNTP server should respond with a 340 status-code indicating that you may post. 
            /// The headers and content of the post can then be sent to the server with a terminating single line containing one period. 
            /// If the message is received correctly, the NNTP server will respond with a 240 status-code.
            /// Our private methods used two private methods, Write and Response. The Write method sends a string of bytes to the NNTP server.
            /// 
            throw new NotImplementedException();
        }
    }
}
