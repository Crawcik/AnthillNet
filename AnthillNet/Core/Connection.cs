using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public struct Connection : IConnection
    {
        internal Connection(IPEndPoint address)
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
        public int MessagesCount { get => this.messages.Count; }
        public IPEndPoint EndPoint { get; }
        internal Socket Socket { get; }
        private List<Packet> messages { get; }
        #endregion

        internal byte[] tempBuffer;

        #region Methods
        internal void Add(byte[] message) => this.messages.Add(new Packet(this,message));
        internal void ClearMessages() => this.messages.Clear();
        public Packet[] GetMessages() => this.messages.ToArray();
        #endregion
    }

    #region
    public interface IConnection
    {
        public IPEndPoint EndPoint { get; }
        public int MessagesCount { get; }
        public Packet[] GetMessages();
    }
    #endregion
}