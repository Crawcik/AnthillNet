﻿using AnthillNet.Core;
using System;
using System.Collections.Generic;
using System.Net;

namespace AnthillNet
{
    public class Host : IDisposable
    {
        #region Properties
        public Base Transport { private set; get; }
        public HostType Type { private set; get; }
        public HostSettings Settings { set; get; }
        #endregion

        #region Initializers
        private Host() { }
        public Host(HostType type) {
            this.Transport = (this.Type = type) switch
            {
                HostType.Server => new Server(),
                HostType.Client => new Client(),
                _ => throw new NotImplementedException()

            };
            this.Settings = new HostSettings()
            {
                Name = null,
                MaxConnections = 0,
                MaxDataSize = 4096,
                TickRate = 8,
                WriteLogsToConsole = true,
                Protocol = ProtocolType.TCP,
                LogPriority = LogType.Error
            };
            this.Transport.OnStop += OnStopped;
            this.Transport.OnReceiveData += OnRevieceMessage;
        }
        #endregion

        #region Public methods
        public void Start(string hostname, ushort port)
        {
            this.Transport.Logging.LogPriority = this.Settings.LogPriority;
            if (this.Settings.Name != null)
                this.Transport.Logging.LogName = this.Settings.Name;
            if (this.Settings.WriteLogsToConsole)
                this.Transport.Logging.OnNetworkLog += OnNetworkLog;
            else
                this.Transport.Logging.OnNetworkLog -= OnNetworkLog;
            this.Transport.Init(this.Settings.Protocol, this.Settings.TickRate);
            IPAddress ip;
            if (!this.ResolveIP(hostname, out ip))
                return;
            this.Transport.Start(ip, port);
        }
        public void Start(ushort port) => this.Start("127.0.0.1", port);
        public void Stop() 
        {
            if (this.Transport.Active)
                this.Transport.Stop();
            else
                this.Transport.Logging.Log($"{this.Transport.Logging.LogName} is already stopped", LogType.Info);
        }
        public void SendTo(Message message, string connection)
        {
            byte[] buf = message.Serialize();
            if (this.Settings.MaxDataSize > buf.Length)
            {
                if (this.Type == HostType.Server)
                {
                    Server server = this.Transport as Server;
                    if(server.Dictionary.ContainsKey(connection))
                        this.Transport.Send(buf, server.Dictionary[connection].EndPoint);
                    else
                        this.Transport.Logging.Log($"Client {connection} isn't connected!", LogType.Warning);

                }
                else if (this.Type == HostType.Client)
                {
                    this.Transport.Send(buf, null);
                }
            }
            else this.Transport.Logging.Log("Message data is too big!", LogType.Error);
        }

        public void Send(Message message)
        {
            byte[] buf = message.Serialize();
            if (this.Settings.MaxDataSize > buf.Length)
            {
                if (this.Type == HostType.Server)
                {
                    Server server = this.Transport as Server;
                    foreach (Connection ip in server.Dictionary.Values)
                        this.Transport.Send(buf, ip.EndPoint);

                }
                else if(this.Type == HostType.Client)
                {
                    this.Transport.Send(buf, null);
                }
            }
            else this.Transport.Logging.Log("Message data is too big!", LogType.Error);
        }
        public void Dispose()
        {
            if (this.Transport.Active)
                this.Transport.ForceStop();
            this.Transport.Dispose();
        }
        #endregion

        #region Private methods
        private void OnStopped(object sender)
        {
            this.Transport.Logging.Log($"Stopped", LogType.Info);
            this.Transport.Dispose();
        }
        private void OnNetworkLog(object sender, NetworkLogArgs e)
        {
            Console.Write($"[{e.Time:HH:mm:ss}]");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write($"[{e.LogName}]");
            switch (e.Priority)
            {
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            Console.Write($"[{e.Priority}]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{e.Message}\n");
        }
        private void OnRevieceMessage(object sender, Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                if (packet.data.Length > this.Transport.MaxMessageSize)
                    this.Transport.Logging.Log($"Received data from {packet.connection.EndPoint} is too big!", LogType.Warning);
                Message message;
                try
                {
                    message = Message.Deserialize(packet.data);
                }
                catch
                {
                    this.Transport.Logging.Log($"Failed deserializing message from {packet.connection.EndPoint}!", LogType.Warning);
                }
            }
        }
        private bool ResolveIP(string hostname, out IPAddress iPAddress)
        {
            switch (Uri.CheckHostName(hostname))
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    if (IPAddress.TryParse(hostname, out iPAddress))
                        return true;
                    break;
                case UriHostNameType.Dns:
                    IPHostEntry hostEntry;
                    if ((hostEntry = Dns.GetHostEntry(hostname)).AddressList.Length > 0)
                    {
                        iPAddress = hostEntry.AddressList[0];
                        return true;
                    }
                    break;
            }
            this.Transport.Logging.Log("Given hostname is invalid!", LogType.Error);
            iPAddress = null;
            return false;
        }
        #endregion
    }
}