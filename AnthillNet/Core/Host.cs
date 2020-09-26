namespace AnthillNet.Core
{
    public abstract class Host
    {
        protected Host() { }

        //Delegates
        public delegate void ConnectedHandler(object sender, string address);
        public delegate void DisconnectedHandler(object sender, string address);
        public delegate void IncomingMessagesHandler(object sender, Message[] messages);
        //Events
        public event ConnectedHandler OnConnect;
        public event DisconnectedHandler OnDisconnect;
        public event IncomingMessagesHandler OnRevieceMessage;
        //Invokers
        protected void ConnectedInvoke(string address) => this.OnConnect?.Invoke(this, address);
        protected void DisconnectedInvoke(string address) => this.OnDisconnect?.Invoke(this, address);
        protected void IncomingMessagesInvoke(Message[] messages) => this.OnRevieceMessage?.Invoke(this, messages);

        internal Base Transport;
        public ProtocolType Protocol;
        public NetworkLog Logging { protected set; get; } = new NetworkLog();
        public abstract void Init(byte tickRate = 32);
        public abstract void Stop();
    }
}
