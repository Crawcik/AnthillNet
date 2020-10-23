﻿using System;
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
                double rest;
                while (!this.ForceOff)
                {
                    DateTime before_tick = DateTime.Now;
                    if (!this.isPause)
                        this.Tick();
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

        //Variables
        private readonly Thread Clock;
        private bool isPause;
        private bool ForceOff;
        protected Socket HostSocket;

        //Controlling functionality
        public virtual void Init(ProtocolType protocol, byte tickRate = 32)
        {
            Protocol = protocol;
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            TickRate = tickRate;
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
        public virtual void Tick() => this.OnTick?.Invoke(this);
        public virtual void ForceStop() => this.Clock.Abort();
        public virtual void Pause() => this.isPause = true;
        public virtual void Resume() => this.isPause = false;
        public virtual void Connect(Connection connection) { if (OnConnect != null) this.OnConnect.Invoke(this, connection); }
        public virtual void Disconnect(Connection connection) { if(OnDisconnect != null) this.OnDisconnect.Invoke(this, connection); }
        public virtual void Send(Message message, IPEndPoint IPAddress) { }
        public virtual void Dispose() => this.HostSocket.Dispose();
    }
}