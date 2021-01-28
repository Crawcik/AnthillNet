using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AnthillNet.Core
{
    public abstract class Base
    {
        #region Properties
        public ProtocolType Protocol { protected set; get; }
        public NetworkLog Logging { protected set; get; }
        public byte TickRate { set; get; } = 8;
        public bool Async { private set; get; }
        public AddressFamily AddressType { set; get; } = AddressFamily.InterNetwork;
        public bool DualChannels { set; get; } = false;
        public int MaxMessageSize { set; get; } = 1024;
        public bool Active => Async ? this.Clock.IsAlive : !this.ForceOff;
        #endregion

        #region Fields
        private Thread Clock;
        private bool isPause;
        private bool ForceOff;
        private bool initialized;
        protected Socket HostSocket;
        #endregion

        #region Controlling functionality
        public virtual void Init(ProtocolType protocol, bool async = true, byte tickRate = 32)
        {
            if (async)
            {
                this.Clock = new Thread(() =>
                {
                    try
                    {
                        double rest;
                        while (!this.ForceOff)
                        {
                            DateTime before_tick = DateTime.Now;
                            if (!this.isPause)
                                this.Tick();
                            if (TickRate != 0)
                                if ((rest = 1000 / TickRate - (DateTime.Now - before_tick).TotalMilliseconds) > 0)
                                    Thread.Sleep((int)rest);
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        this.Logging.Log($"Handling thread interrupted", LogType.Debug);
                    }
                    finally
                    {
                        this.ForceOff = true;
                    }
                    this.OnStop?.Invoke(this);
                })
                { IsBackground = true };
            }
            this.Logging = new NetworkLog();
            this.Async = async;
            this.Protocol = protocol;
            this.Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Info);
            this.TickRate = tickRate;
            if (this.DualChannels)
                this.AddressType = AddressFamily.InterNetworkV6;
            this.initialized = true;
        }

        public virtual void Start(IPAddress ip, ushort port)
        {
            if(!this.initialized || this.Active)
                return;
            this.AddressType = ip.AddressFamily;
            this.ForceOff = false;
            this.isPause = false;
            if (this.HostSocket == null)
            {
                if (this.Protocol == ProtocolType.TCP)
                    this.HostSocket = new Socket(AddressType, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                else if (this.Protocol == ProtocolType.UDP)
                    this.HostSocket = new Socket(AddressType, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            }
            if (this.DualChannels)
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    this.HostSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                else
                    this.Logging.Log("Address type has invalid type to use dual channels", LogType.Warning);
            }
            if (this.Async && !this.Clock.IsAlive)
            {
                this.Clock.Start();
                this.Async = true;
            }
            this.Logging.Log($"Starting host", LogType.Info);
        }
        public virtual void Stop()
        {
            if (!this.Active) return;
            this.Logging.Log($"Stopped", LogType.Info);
            this.HostSocket.Close();
            this.ForceOff = true;
        }
        public virtual void Tick() => this.OnTick?.Invoke(this);
        public virtual void ForceStop() {
            this.Logging.Log($"Force stopping...", LogType.Debug);
            if (this.Active && this.Async)
                this.Clock.Interrupt();
            this.ForceOff = true;
            this.HostSocket.Close();
            this.Logging.Log($"Stopped", LogType.Info);
        }
        public virtual void Pause() => this.isPause = true;
        public virtual void Resume() => this.isPause = false;
        public virtual void Connect(Connection connection) { if (this.OnConnect != null) this.OnConnect.Invoke(this, connection); }
        public virtual void Disconnect(Connection connection) { if(this.OnDisconnect != null) this.OnDisconnect.Invoke(this, connection); }
        public virtual void Send(byte[] buffer, IPEndPoint IPAddress) { }
        public virtual void Dispose() 
        {
            if(!initialized)
                return;
#if !(NET20 || NET35)
            this.HostSocket.Dispose();
#endif
            this.HostSocket = null;
            initialized = false;
            Logging = null;
            Clock = null;
        }
        #endregion

        #region Delegates
        public delegate void TickHandler(object sender);
        public delegate void ConnectHandler(object sender, Connection connection);
        public delegate void DisconnectHandler(object sender, Connection connection);
        public delegate void IncomingMessagesHandler(object sender, Packet[] packets);
        public delegate void InternalHostErrorHandler(object sender, System.Exception exception);
        public delegate void StopHandler(object sender);
        #endregion

        #region Events
        public event TickHandler OnTick;
        public event ConnectHandler OnConnect;
        public event DisconnectHandler OnDisconnect;
        public event IncomingMessagesHandler OnReceiveData;
        public event InternalHostErrorHandler OnInternalHostError;
        public event StopHandler OnStop;
        #endregion

        #region Event Invokers
        protected void IncomingMessagesInvoke(Connection connection)
        {
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