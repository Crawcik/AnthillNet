using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public struct Connection
    {
        public Connection(IPEndPoint address)
        {
            this.EndPoint = address;
            this.messages = new List<Message>();
            this.Socket = null;
            this.TempBuffer = null;
        }
        internal Connection(Socket socket)
        {
            this.EndPoint = socket.RemoteEndPoint as IPEndPoint;
            this.messages = new List<Message>();
            this.Socket = socket;
            this.TempBuffer = null;
        }
        public IPEndPoint EndPoint { private set; get; }
        internal Socket Socket { private set; get; }
        internal byte[] TempBuffer { set; get; }

        private List<Message> messages;

        public int MessagesCount => messages.Count;

        internal void Add(Message message) => messages.Add(message);
        internal void ClearMessages() => messages.Clear();

        public Message[] GetMessages()
        {
            Message[] results = messages.ToArray();
            return results;
        }

    }
}
