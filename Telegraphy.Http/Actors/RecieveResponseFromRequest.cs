using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Http
{
    public class RecieveResponseFromRequest<MsgType> : IActor where MsgType : class
    {
        string url;
        Func<string, IActorMessage, NameValueCollection> queryBuilder;
        Func<IActorMessage, HttpContent> contentBuilder;
        HttpMethod method;

        public RecieveResponseFromRequest(string url
            , HttpMethod method
            , Func<string, IActorMessage, NameValueCollection> queryBuilder
            , Func<IActorMessage,HttpContent> contentBuilder)
        {
            this.queryBuilder = queryBuilder;
            this.contentBuilder = contentBuilder;
            this.method = method;
            this.url = url;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            string requestURI = url;
            if (null != queryBuilder)
            {
                NameValueCollection items = queryBuilder(url, msg);
                //TODO turn NameValueCollection into requestURI
                throw new NotImplementedException("TODO turn NameValueCollection into requestURI");
            }

            HttpContent content = null;
            if (null != contentBuilder)
            {
                content = contentBuilder(msg);
            }

            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestURI);
                requestMessage.Content = content;

                // TODO timeout??
                HttpResponseMessage response = client.SendAsync(requestMessage).Result;
                response.EnsureSuccessStatusCode();
                if (null != msg.Status && null != response.Content)
                {
                    if (typeof(MsgType) == typeof(string))
                        msg.Status.SetResult(response.Content.ReadAsStringAsync().Result.ToActorMessage());
                    else if (typeof(MsgType) == typeof(byte[]))
                        msg.Status.SetResult(response.Content.ReadAsByteArrayAsync().Result.ToActorMessage());
                    else
                    {
                        byte[] msgBytes = response.Content.ReadAsByteArrayAsync().Result;
                        var t = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(msgBytes));
                        msg.Status.SetResult(t.Result as IActorMessage);
                    }
                }
            }
            return true;
        }
    }
}
