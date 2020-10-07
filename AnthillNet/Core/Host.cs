using System;

namespace AnthillNet.Core
{
    public abstract class Host : IDisposable
    {
        protected Host() { }

        //Delegates
        public delegate void ConnectedHandler(object sender, string address);
        public delegate void DisconnectedHandler(object sender, string address);
        public delegate void IncomingMessagesHandler(object sender, Connection messages);
        //Events
        public event ConnectedHandler OnConnect;
        public event DisconnectedHandler OnDisconnect;
        public event IncomingMessagesHandler OnRevieceMessage;
        //Properties
        internal Base Transport { set; get; }
        public ProtocolType Protocol { protected set; get; }
        public NetworkLog Logging { protected set; get; } = new NetworkLog();
        public virtual void Init(byte tickRate = 32)
        {
            this.Transport.OnConnect += OnHostConnect;
            this.Transport.OnDisconnect += OnHostDisconnect;
            this.Transport.OnTick += OnTick;
            this.Transport.OnStop += OnStop;
            this.Transport.OnIncomingMessages += OnIncomingMessages;
            this.Transport.OnInternalHostError += OnHostError;
        }
        public virtual void Stop(Message[] additional_packages = null) => OnStop();
        public abstract void Dispose();

        //Protected events
        protected virtual void OnHostConnect(Connection connection) => this.OnConnect?.Invoke(this, connection.EndPoint.ToString());
        protected virtual void OnHostDisconnect(Connection connection) => this.OnDisconnect?.Invoke(this, connection.EndPoint.ToString());
        protected virtual void OnTick() { }
        protected virtual void OnStop() => Logging.Log($"Stopped");
        protected virtual void OnIncomingMessages(Connection connection) => this.OnRevieceMessage?.Invoke(this, connection);
        protected virtual void OnHostError(Exception exception) => this.Logging.Log("Internal error occured: " + exception.ToString(), LogType.Error);
    }
}
