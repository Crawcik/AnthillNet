using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AnthillNet.Core
{
    public abstract partial class Base
    {
        protected Base() => Clock = new Thread(() =>
        {
            try
            {
                double rest;
                while (!this.ForceOff)
                {
                    DateTime before_tick = DateTime.Now;
                    if (!this.isPause)
                        this.Tick();
                    if(TickRate != 0)
                        if ((rest = 1000 / TickRate - (DateTime.Now - before_tick).TotalMilliseconds) > 0)
                            Thread.Sleep((int)rest);
                }
            }
            finally
            {
                this.ForceOff = true;
            }
            this.OnStop?.Invoke(this);
        })
        { IsBackground = true };

        #region Variables
        private readonly Thread Clock;
        private bool isPause;
        private bool ForceOff;
        protected Socket HostSocket;
        #endregion

        #region Controlling functionality
        public virtual void Init(ProtocolType protocol, byte tickRate = 32)
        {
            this.Protocol = protocol;
            this.Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Info);
            this.TickRate = tickRate;
            if (protocol == ProtocolType.TCP)
                this.HostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            else if(protocol == ProtocolType.UDP)
                this.HostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        }
        public virtual void Start(IPAddress ip, ushort port, bool run_clock = true)
        {
            this.HostIP = ip;
            this.Port = port;
            this.ForceOff = false;
            this.isPause = false;
            if(run_clock)
                this.Clock.Start();
            this.Logging.Log($"Started at {port} port", LogType.Info);
        }
        public virtual void Stop()
        {
            this.Logging.Log($"Stopped", LogType.Info);
            this.ForceOff = true;
        }
        public virtual void Tick() => this.OnTick?.Invoke(this);
        public virtual void ForceStop() { 
            if(this.Active)
                this.Clock.Abort();
            this.Logging.Log($"Stopped", LogType.Info);
        }
        public virtual void Pause() => this.isPause = true;
        public virtual void Resume() => this.isPause = false;
        public virtual void Connect(Connection connection) { if (this.OnConnect != null) this.OnConnect.Invoke(this, connection); }
        public virtual void Disconnect(Connection connection) { if(this.OnDisconnect != null) this.OnDisconnect.Invoke(this, connection); }
        public virtual void Send(byte[] buffer, IPEndPoint IPAddress) { }
        public virtual void Dispose() 
        {
#if !(NET20 || NET35)
            this.HostSocket.Dispose();
#endif
            this.HostSocket = null;
        }
#endregion
    }
}