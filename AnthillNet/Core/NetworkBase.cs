using System;
using System.Threading;

namespace AnthillNet.Core
{
    internal abstract class Base
    {
        protected Base() => Clock = new Thread(() =>
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
            this.OnStop?.Invoke();
        })
        { IsBackground = true };

        //Variables
        private readonly Thread Clock;
        private bool isPause;
        private bool ForceOff;

        public string Hostname { private set; get; }
        public ushort Port { private set; get; }
        public byte TickRate { set; get; }
        public int MaxMessageSize { set; get; } = 1024;
        public bool Active => this.Clock.IsAlive;

        //Controlling functionality
        public virtual void Start(string hostname, ushort port)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.ForceOff = false;
            this.isPause = false;
            this.Clock.Start();
        }
        public virtual void Stop() => this.ForceOff = true;
        public virtual void ForceStop() { this.Clock.Abort(); this.OnStop?.Invoke(); }
        public virtual void Pause() => this.isPause = true;
        public virtual void Resume() => this.isPause = false;
        public virtual void Connect(Connection connection) => this.OnConnect.Invoke(connection);
        public virtual void Disconnect(Connection connection) => this.OnDisconnect.Invoke(connection);
        public virtual void Send(Message message, string IPAddress) { if (this.MaxMessageSize < message.lenght) throw new Exception("Message data is too big"); }

        //Delegates
        public delegate void TickHander();
        public delegate void ConnectHandler(Connection connection);
        public delegate void DisconnectHandler(Connection connection);
        public delegate void IncomingMessagesHandler(Connection connection);
        public delegate void StopHandler();

        //Events
        public event TickHander OnTick;
        public event ConnectHandler OnConnect;
        public event DisconnectHandler OnDisconnect;
        public event IncomingMessagesHandler OnIncomingMessages;
        public event StopHandler OnStop;

        //Events Invokers
        protected virtual void Tick() => this.OnTick?.Invoke();
        protected void IncomingMessagesInvoke(Connection connection) => this.OnIncomingMessages?.Invoke(connection);
    }
}