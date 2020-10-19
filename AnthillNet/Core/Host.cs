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
        public delegate void TickHander();
        public delegate void ConnectHandler(Connection connection);
        public delegate void DisconnectHandler(Connection connection);
        public delegate void IncomingMessagesHandler(Connection connection);
        public delegate void InternalHostErrorHandler(Exception exception);
        public delegate void StopHandler();

        //Events
        public event TickHander OnTick;
        public event ConnectHandler OnConnect;
        public event DisconnectHandler OnDisconnect;
        public event IncomingMessagesHandler OnIncomingMessages;
        public event InternalHostErrorHandler OnInternalHostError;
        public event StopHandler OnStop;

        //Events Invokers
        protected virtual void Tick() => this.OnTick?.Invoke();
        protected void IncomingMessagesInvoke(Connection connection) => this.OnIncomingMessages?.Invoke(connection);
        protected void InternalHostErrorInvoke(Exception exception) => this.OnInternalHostError?.Invoke(exception);
    }
}
