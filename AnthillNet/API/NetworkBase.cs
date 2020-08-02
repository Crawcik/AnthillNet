using System;
using System.Threading;

namespace AnthillNet.API
{
    public abstract class Base
    {
        protected Base() => Clock = new Thread(() =>
        {
            while (!ForceOff)
            {
                int before_tick = DateTime.Now.Millisecond;
                if (!isPause)
                    Tick();
                if (TickRate - (DateTime.Now.Millisecond - before_tick) > 0)
                    Thread.Sleep(1 / (TickRate - (DateTime.Now.Millisecond - before_tick)));
            }
            OnStop?.Invoke();
        })
        { IsBackground = true };

        //Variables
        private Thread Clock;
        public string hostname { private set; get; }
        public ushort port { private set; get; }
        private bool ForceOff;
        public bool isPause { private set; get; }
        public byte TickRate;
        protected bool Active => Clock.IsAlive;

        //Controlling functionality
        public virtual void Start(string hostname, ushort port)
        {
            this.hostname = hostname;
            this.port = port;
            ForceOff = false;
            isPause = false;
            Clock.Start();

        }
        public virtual void Stop() => ForceOff = true;
        public virtual void ForceStop() { Clock.Abort(); OnStop?.Invoke(); }
        public virtual void Pause() => isPause = true;
        public virtual void Resume() => isPause = false;

        //Delegates
        public delegate void TickHander();
        public delegate void ConnectionStabilizedHandler(Connection connection);
        public delegate void StopHandler();

        //Events
        public event TickHander OnTick;
        public event ConnectionStabilizedHandler OnConnectionStabilized;
        public event StopHandler OnStop;

        //Events Invokers
        protected virtual void Tick() => OnTick?.Invoke();
        protected void ConnectionStabilizedInvoke(Connection connection) => OnConnectionStabilized?.Invoke(connection);

    }
}