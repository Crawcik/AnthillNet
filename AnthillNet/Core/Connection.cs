using System.Collections.Generic;
using System.Net;

namespace AnthillNet.Core
{
    public struct Connection
    {
        public Connection(EndPoint EndPoint)
        {
            this.EndPoint = EndPoint as IPEndPoint;
            this.messages = new List<Message>();
        }
        public IPEndPoint EndPoint { private set; get; }
        private readonly List<Message> messages;
        public int MessagesCount => messages.Count;
        internal void Add(Message message) => messages.Add(message);
        internal void ClearMessages() => messages.Clear();
        internal void ClearData()
        {
            this.EndPoint = null;
            messages.Clear();
        }
        public Message[] GetMessages()
        {
            Message[] results = messages.ToArray();
            return results;
        }

    }
}
