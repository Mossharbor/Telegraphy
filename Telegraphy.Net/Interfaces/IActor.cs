using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    public interface IActor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns>true if the message was delivered</returns>
        /// <remarks>Does not indicated if the message was processed.</remarks>
        bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage;
    }
}
