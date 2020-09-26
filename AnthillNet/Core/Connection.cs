using System.Collections.Generic;
using System.Net;

namespace AnthillNet.Core
{
    public struct Connection
    {
        public Connection(EndPoint EndPoint)
        {
            this.EndPoint = EndPoint as IPEndPoint;
            this.messages_received = new List<Message>();
            this.messages_tosend = new List<Message>();
        }
        public IPEndPoint EndPoint { private set; get; }
        private readonly List<Message> messages_received;
        private readonly List<Message> messages_tosend;
        public int MessagesCount => messages_received.Count;

        public void AddReceived(Message message) => messages_received.Add(message);
        public void AddToSend(Message message) => messages_tosend.Add(message);
        public Message[] GetMessagesReceived()
        {
            Message[] results = messages_received.ToArray();
            messages_received.Clear();
            return results;
        }
        public Message[] GetMessagesToSend()
        {
            Message[] results = messages_received.ToArray();
            messages_received.Clear();
            return results;
        }

    }
}
