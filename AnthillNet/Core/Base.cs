﻿using System;
using System.Net;
using System.Threading;

namespace AnthillNet.Core
{
    internal abstract class Base
    {
        protected Base() => Clock = new Thread(() =>
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
        public virtual void ForceStop() => this.Clock.Abort();
        public virtual void Pause() => this.isPause = true;
        public virtual void Resume() => this.isPause = false;
        public virtual void Connect(IPEndPoint endPoint) => this.OnConnect.Invoke(new Connection(endPoint));
        public virtual void Disconnect(Connection connection) => this.OnDisconnect.Invoke(connection);
        public virtual void Send(Message message, string IPAddress) { if (this.MaxMessageSize < message.Serialize().Length) InternalHostErrorInvoke(new Exception("Message data is too big")); }

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