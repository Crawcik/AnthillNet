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

        #region Properties
        public int MessagesCount => this.messages.Count;
        public IPEndPoint EndPoint { private set; get; }
        internal Socket Socket { private set; get; }
        internal byte[] TempBuffer { set; get; }
        #endregion

        private List<Message> messages;

        #region Methods
        internal void Add(Message message) => this.messages.Add(message);
        internal void ClearMessages() => this.messages.Clear();

        public Message[] GetMessages()
        {
            Message[] results = this.messages.ToArray();
            return results;
        }
        #endregion
    }
}
