using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AnthillNet.Core
{
    public abstract partial class Host
    {
        protected Host() => Clock = new Thread(() =>
        {
            try
            {
                int rest = 1;
                while (!this.ForceOff)
                {
                    int before_tick = DateTime.Now.Millisecond;
                    if (!this.isPause)
                        this.Tick();
                    if ((rest = this.TickRate - (DateTime.Now.Millisecond - before_tick)) > 0)
                        Thread.Sleep(1 / rest == 0 ? TickRate : rest);
                }
            }
            finally
            {
                this.ForceOff = true;
            }
            this.OnStop?.Invoke();
        })
        { IsBackground = true };

        //Variables
        private readonly Thread Clock;
        private bool isPause;
        private bool ForceOff;
        protected Socket HostSocket;

        //Controlling functionality
        public virtual void Init(ProtocolType protocol)
        {
            if (protocol == ProtocolType.TCP)
                HostSocket = new Socket(SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            else if(protocol == ProtocolType.UDP)
                HostSocket = new Socket(SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        }
        public virtual void Start(string hostname, ushort port)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.ForceOff = false;
            this.isPause = false;
            this.Clock.Start();
        }
        public virtual void Stop() => this.ForceOff = true;
        public virtual void ForceStop() => this.Clock.Abort();
        public virtual void Pause() => this.isPause = true;
        public virtual void Resume() => this.isPause = false;
        public virtual void Connect(Connection connection) => this.OnConnect.Invoke(connection);
        public virtual void Disconnect(Connection connection) => this.OnDisconnect.Invoke(connection);
        public virtual void Send(Message message, IPEndPoint IPAddress) { if (this.MaxMessageSize < message.Serialize().Length) InternalHostErrorInvoke(new Exception("Message data is too big")); }
    }
}