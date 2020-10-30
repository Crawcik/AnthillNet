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

        private ulong destiny_avalible;

        public Interpreter() => destiny_avalible = reservedIDs;

        public void ResolveMessage(Message message)
        {
            if (reservedIDs > message.destiny)
            {
                switch ((InternalFuctionsID)message.destiny)
                {
                    case InternalFuctionsID.Ping:
                        OnMessageGenerate?.Invoke(this, new Message((ulong)InternalFuctionsID.Pong, null);
                        break;
                    case InternalFuctionsID.Pong:

                        break;
                    case InternalFuctionsID.Order:
                        ((Order)message.data).Invoke();
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
