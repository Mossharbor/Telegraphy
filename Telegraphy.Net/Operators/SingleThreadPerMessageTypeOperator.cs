using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net
{
    using System.Threading;
    using System.Collections.Concurrent;

    public class SingleThreadPerMessageTypeOperator : IOperator
    {
        ConcurrentDictionary<Type, Action<Exception>> _exceptionTypeToHandler = new ConcurrentDictionary<Type, Action<Exception>>();
        ConcurrentDictionary<Type, LocalOperator> messageQueues = new ConcurrentDictionary<Type, LocalOperator>();

        Func<ILocalSwitchboard> createSwitchBoard = null;
        bool isDisabled = false;

        public SingleThreadPerMessageTypeOperator() :this(new Func<ILocalSwitchboard>(()=>new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors)))
        {
        }

        private SingleThreadPerMessageTypeOperator(Func<ILocalSwitchboard> createSwitchBoard)
        {
            this.createSwitchBoard = createSwitchBoard;
            this.ID = 0;
        }

        #region Operator

        public ulong Count { get { return 0; } }

        public long ID { get; set; }
        
        public bool IsAlive()
        {
            bool areAllDisbled = true;

            foreach (var t in messageQueues.Values)
            {
                if (t.IsAlive())
                    areAllDisbled = false;
            }

            return !areAllDisbled;
        }

        public void Kill()
        {
            foreach (var key in messageQueues.Values)
                key.Kill();
        }

        public void AddMessage(IActorMessage msg)
        {
            if (!messageQueues.ContainsKey(msg.GetType()))
            {
                LocalOperator op = new LocalOperator(createSwitchBoard());
                if (!messageQueues.TryAdd(msg.GetType(), op))
                    throw new FailedMessageEnqueException(msg.GetType().ToString());
            }
            else
                messageQueues[msg.GetType()].AddMessage(msg);
        }

        public IActorMessage GetMessage()
        {
            // We are farming this out to the dictionary of local operators. we dont actually have any ourselves
            throw new NotImplementedException();
        }
        
        public ICollection<ILocalSwitchboard> Switchboards
        {
            get
            {
                List<ILocalSwitchboard> switchBoards = new List<ILocalSwitchboard>();
                foreach (var op in messageQueues.Values)
                    switchBoards.AddRange(op.Switchboards);
                return switchBoards;
            }
        }

        public bool WaitTillEmpty(TimeSpan timeout)
        {
            int millisecondsToWaitPerSubOperator = (int)Math.Max(10,messageQueues.Values.Count/timeout.TotalMilliseconds);

            foreach(var key in messageQueues.Values)
                key.WaitTillEmpty(new TimeSpan(0,0,0,0,millisecondsToWaitPerSubOperator));

            return true;
        }

        public void Register(Type exceptionType, Action<Exception> handler)
        {
            while (!_exceptionTypeToHandler.TryAdd(exceptionType, handler))
                _exceptionTypeToHandler.TryAdd(exceptionType, handler);
        }

        public void Register(Type exceptionType, Func<Exception, IActor, IActorMessage, IActorInvocation, IActor> handler)
        {
            foreach (var key in messageQueues.Values)
                key.Register(exceptionType, handler);
        }
        #endregion

        #region IActor
        public bool OnMessageRecieved<T>(T msg) where T : class, IActorMessage
        {
            this.AddMessage(msg);
            return true;;
        }
        
        #endregion
    }
}
