using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthillNet.Events
{
    [Serializable]
    public class Interpreter
    {
        public const ulong reservedIDs = 100;
        public delegate void MessageGenerate(object sender, Message message);
        public event MessageGenerate OnMessageGenerate;
        internal event MessageGenerate OnEventIncoming;
        private ulong destiny_avalible;
        public bool isServer { private set; get; }
        public bool isClient { private set; get; }

        public Interpreter(bool server, bool client) 
        {
            this.destiny_avalible = reservedIDs;
            this.isServer = server;
            this.isClient = client;
        }

        public void ResolveMessage(Message message)
        {
            if (reservedIDs > message.destiny)
            {
                switch ((InternalFuctionsID)message.destiny)
                {
                    case InternalFuctionsID.Ping:
                        OnMessageGenerate?.Invoke(this, new Message((ulong)InternalFuctionsID.Pong, null));
                        break;
                    case InternalFuctionsID.Pong:
                        
                        break;
                    case InternalFuctionsID.Order:
                        Order order = (Order)message.data;
                        order.Invoke(this.isServer, this.isClient);
                        break;
                    case InternalFuctionsID.Event:
                        OnEventIncoming?.Invoke(this, message);
                        break;
                    default:
                        return;
                }
            }
            else
            {
                
            }
        }

        internal void PrepareOrder(Order order) => OnMessageGenerate?.Invoke(this, new Message((ulong)InternalFuctionsID.Order, order));
        internal void PrepareEvent(EventCommand order) => OnMessageGenerate?.Invoke(this, new Message((ulong)InternalFuctionsID.Event, order));
        public Order GetOrderFunction() => new Order(this);

        private enum InternalFuctionsID
        {
            NULL = 0,
            Ping = 1,
            Pong = 2,
            Order = 10,
            Event = 20
        }
    }
}
