using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System.Collections.Concurrent;
using Azure.Messaging.EventHubs;

namespace Telegraphy.Azure
{
    public class EventHubBaseOperator<MsgType> : IOperator where MsgType:class
    {
        internal const int DefaultDequeueMaxCount = 1;
        internal const int DefaultConcurrency = 3;

        private ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        private int maxDequeueCount = 1;
        private EventHubDataSubscriber EventHubMsgReciever;
        private EventHubDataPublisher EventHubClient;
        private ConcurrentQueue<IActorMessage> msgQueue = new ConcurrentQueue<IActorMessage>();

        internal EventHubBaseOperator(ILocalSwitchboard switchBoard, EventHubDataSubscriber eventHubMsgReciever)
        {
            this.Switchboards.Add(switchBoard);
            switchBoard.Operator = this;
            this.EventHubMsgReciever = eventHubMsgReciever;
            this.ID = 0;
        }

        internal EventHubBaseOperator(EventHubDataPublisher eventHubClient)
        {
            this.EventHubClient = eventHubClient;
            this.ID = 0;
        }

        private static IActorMessage ConvertToActorMessage(byte[] msgBytes)
        {
            IActorMessage msg = null;
            if (typeof(MsgType) == typeof(string))
                msg = Encoding.UTF8.GetString(msgBytes).ToActorMessage();
            else if(typeof(MsgType) == typeof(byte[]))
                    msg = msgBytes.ToActorMessage();
            else
            {
                var t = Telegraph.Instance.Ask(new DeserializeMessage<IActorMessage>(msgBytes));
                msg = t.Result as IActorMessage;
            }
            
            return msg;
        }

        #region IOperator

        public long ID { get; set; }

        public ulong Count { get { return (ulong)msgQueue.Count; } }

        private List<ILocalSwitchboard> switchboards = new List<ILocalSwitchboard>();
        public ICollection<ILocalSwitchboard> Switchboards { get { return switchboards; } }
        
        public void AddMessage(IActorMessage msg)
        {
            try
            {
                EventData eventData = SendBytesToEventHub.BuildMessage<MsgType>(msg);
                EventHubClient.Send(eventData);
            
                if (null == msg.Status)
                    msg.Status = new TaskCompletionSource<IActorMessage>();

                msg.Status?.SetResult(new EventHubMessage(eventData));
            }
            catch (Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);

                if (null != msg.Status && !msg.Status.Task.IsCanceled)
                    msg.Status.TrySetException(ex);
            }
        }

        public IActorMessage GetMessage()
        {
            try
            {
                if (1 == maxDequeueCount)
                {
                    var recievedSingle = this.EventHubMsgReciever.Recieve(maxDequeueCount, null);
                    if (null == recievedSingle || 0 == recievedSingle.Count())
                        return null;

                    byte[] msgBytes = null;
                    msgBytes = recievedSingle.First().Body.ToArray();
                    return ConvertToActorMessage(msgBytes);
                }

                while (0 != msgQueue.Count)
                {
                    IActorMessage msg = null;
                    if (msgQueue.TryDequeue(out msg))
                        return msg;
                }

                var recievedList = this.EventHubMsgReciever.Recieve(maxDequeueCount, null);
                if (null == recievedList || 0 == recievedList.Count())
                    return null;

                for (int i = 1; i < recievedList.Count(); ++i)
                {
                    byte[] msgBytes = recievedList.ElementAt(i).Body.ToArray();
                    msgQueue.Enqueue(ConvertToActorMessage(msgBytes));
                }

                // save the first one to return
                return ConvertToActorMessage(recievedList.ElementAt(0).Body.ToArray());
            }
            catch (Exception ex)
            {
                Exception foundEx = null;
                var handler = this.FindExceptionHandler(_exceptionTypeToHandler, ex, out foundEx);

                if (null != handler)
                    handler.Invoke(foundEx);
                return null;
            }
        }
        
        public void Kill()
        {
            if (null != EventHubMsgReciever)
            {
                EventHubMsgReciever.Close();
            }

            foreach(var switchBoard in this.Switchboards)
                switchBoard.Disable();
        }
        
        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }
        
        public bool WaitTillEmpty(TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start) < timeout && (0 != this.msgQueue.Count))
            {
                System.Threading.Thread.Sleep(1000);
            }

            return ((DateTime.Now - start) <= timeout);
        }
        #endregion

        #region IActor
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            AddMessage(msg);
            return true;
        }
        #endregion
    }
}
