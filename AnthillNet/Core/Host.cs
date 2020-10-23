using System;
using System.Net;

namespace AnthillNet.Core
{
    public abstract partial class Host
    {
        //Properties
        public ProtocolType Protocol { protected set; get; }
        public NetworkLog Logging { protected set; get; } = new NetworkLog();
        public string Hostname { private set; get; }
        public ushort Port { private set; get; }
        public byte TickRate { set; get; }
        public int MaxMessageSize { set; get; } = 1024;
        public bool Active => this.Clock.IsAlive;

        //Delegates
        public delegate void TickHander(object sender);
        public delegate void ConnectHandler(object sender, Connection connection);
        public delegate void DisconnectHandler(object sender, Connection connection);
        public delegate void IncomingMessagesHandler(object sender, Connection connection);
        public delegate void InternalHostErrorHandler(object sender, Exception exception);
        public delegate void StopHandler(object sender);

        //Events
        public event TickHander OnTick;
        public event ConnectHandler OnConnect;
        public event DisconnectHandler OnDisconnect;
        public event IncomingMessagesHandler OnReceiveMessages;
        public event InternalHostErrorHandler OnInternalHostError;
        public event StopHandler OnStop;

        //Events Invokers
        protected void IncomingMessagesInvoke(Connection connection) {
            this.Logging.Log($"Message from {connection.EndPoint}: Count {connection.MessagesCount}", LogType.Debug);
            if (this.OnReceiveMessages != null)
                this.OnReceiveMessages?.Invoke(this, connection);
        }
        protected void InternalHostErrorInvoke(Exception exception) 
        {
            this.Logging.Log(exception.Message, LogType.Error);
            if (this.OnInternalHostError != null)
                this.OnInternalHostError?.Invoke(this, exception);
        }
    }
}
