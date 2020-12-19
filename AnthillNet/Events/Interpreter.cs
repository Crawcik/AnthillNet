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
        public delegate void MessageGenerate(object sender, Message message, string target);
        public event MessageGenerate OnMessageGenerate;
        public bool isServer { private set; get; }
        public bool isClient { private set; get; }

        public Interpreter(bool server, bool client) 
        {
            this.isServer = server;
            this.isClient = client;
        }

        public void ResolveMessage(Message message)
        {
            switch ((InternalFuctionsID)message.destiny)
            {
                case InternalFuctionsID.Ping:
                    OnMessageGenerate?.Invoke(this, new Message((ulong)InternalFuctionsID.Pong, null), null);
                    break;
                case InternalFuctionsID.Pong:

                    break;
                case InternalFuctionsID.Order:
                    Order order = (Order)message.data;
                    order.Invoke(this.isServer, this.isClient);
                    break;
                default:
                    return;
            }
        }

        internal void PrepareOrder(Order order) => OnMessageGenerate?.Invoke(this, new Message((ulong)InternalFuctionsID.Order, order), order.target);
        public Order GetOrderFunction() => new Order(this);
        private enum InternalFuctionsID
        {
            NULL = 0,
            Ping = 1,
            Pong = 2,
            Order = 10
        }
    }
}
