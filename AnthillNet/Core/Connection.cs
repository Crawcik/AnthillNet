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
            this.messages = new List<Packet>();
            this.Socket = null;
            this.tempBuffer = null;
        }
        internal Connection(Socket socket)
        {
            this.EndPoint = socket.RemoteEndPoint as IPEndPoint;
            this.messages = new List<Packet>();
            this.Socket = socket;
            this.tempBuffer = null;
        }

        #region Properties
        public int MessagesCount => this.messages.Count;
        public IPEndPoint EndPoint { private set; get; }
        internal Socket Socket { private set; get; }
        internal byte[] tempBuffer;
        #endregion

        private readonly List<Packet> messages;

        #region Methods
        internal void Add(byte[] message) => this.messages.Add(new Packet(this,message));
        internal void ClearMessages() => this.messages.Clear();
        public Packet[] GetMessages() => this.messages.ToArray();
        #endregion
    }
}