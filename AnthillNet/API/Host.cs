namespace AnthillNet.API
{
    public abstract class Host
    {
        //Delegates
        public delegate void ConnectedHandler(object sender, string address);
        public delegate void DisconnectedHandler(object sender, string address);
        public delegate void IncomingMessagesHandler(object sender, Message[] messages);
        //Events
        public event ConnectedHandler OnConnect;
        public event DisconnectedHandler OnDisconnect;
        public event IncomingMessagesHandler OnRevieceMessage;
        //Invokers
        protected void ConnectedInvoke(string address) => OnConnect?.Invoke(this, address);
        protected void DisconnectedInvoke(string address) => OnDisconnect?.Invoke(this, address);
        protected void IncomingMessagesInvoke(Message[] messages) => OnRevieceMessage?.Invoke(this, messages);

        protected Base Transport;
        public ProtocolsType Protocol;
        public abstract void Init(byte tickRate = 32);
        public abstract void Stop();
    }
}
