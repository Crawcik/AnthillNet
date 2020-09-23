using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace AnthillNet.Core
{
    internal struct Connection
    {
        public Connection(EndPoint EndPoint)
        {
            this.EndPoint = EndPoint as IPEndPoint;
            this.messages = new List<Message>();
        }
        public IPEndPoint EndPoint { private set; get; }
        private readonly List<Message> messages;

        public void Add(Message message) => messages.Add(message);
        public int MessagesCount => messages.Count;
        public Message[] GetMessages()
        {
            Message[] results = messages.ToArray();
            messages.Clear();
            return results;
        }

    }
}
