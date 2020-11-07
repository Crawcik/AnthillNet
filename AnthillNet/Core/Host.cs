namespace AnthillNet.Core
{
    public abstract partial class Base
    {
        #region Properties
        public ProtocolType Protocol { protected set; get; }
        public NetworkLog Logging { protected set; get; } = new NetworkLog();
        public System.Net.IPAddress HostIP { private set; get; }
        public ushort Port { private set; get; }
        public byte TickRate { set; get; }
        public bool Async { set; get; }
        public int MaxMessageSize { set; get; } = 1024;
        public bool Active => this.Clock.IsAlive;
        #endregion

        #region Delegates
        public delegate void ConnectHandler(object sender, Connection connection);
        public delegate void DisconnectHandler(object sender, Connection connection);
        public delegate void IncomingMessagesHandler(object sender, Packet[] packets);
        public delegate void InternalHostErrorHandler(object sender, System.Exception exception);
        public delegate void StopHandler(object sender);
        #endregion

        #region Events
        public event ConnectHandler OnConnect;
        public event DisconnectHandler OnDisconnect;
        public event IncomingMessagesHandler OnReceiveData;
        public event InternalHostErrorHandler OnInternalHostError;
        public event StopHandler OnStop;
        #endregion

        #region Event Invokers
        protected void IncomingMessagesInvoke(Connection connection) {
            this.Logging.Log($"Message from {connection.EndPoint}: Count {connection.MessagesCount}", LogType.Debug);
            if (this.OnReceiveData != null)
                this.OnReceiveData?.Invoke(this, connection.GetMessages());
            connection.ClearMessages();
        }
        protected void InternalHostErrorInvoke(System.Exception exception) 
        {
            this.Logging.Log(exception.Message, LogType.Error);
            if (this.OnInternalHostError != null)
                this.OnInternalHostError?.Invoke(this, exception);
        }
        #endregion
    }
}
