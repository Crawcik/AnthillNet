﻿using AnthillNet.Core;
using System;
using System.Collections.Generic;
using System.Net;

namespace AnthillNet
{
    public class Host
    {
        public Base Transport { private set; get; }
        public HostType Type { private set; get; }
        public HostSettings Settings { private set; get; }

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
                MaxConnections = 0,
                MaxDataSize = 4096,
                TickRate = 8,
                WriteLogsToConsole = false,
                Protocol = ProtocolType.TCP,
                LogPriority = LogType.Error
            };
            
        }

        public void Start(string hostname, ushort port)
        {
            this.Transport.Logging.LogPriority = this.Settings.LogPriority;
            if(Settings.WriteLogsToConsole)
                this.Transport.Logging.OnNetworkLog += OnNetworkLog;
            this.Transport.OnReceiveMessages += OnRevieceMessage;
            this.Transport.Init(this.Settings.Protocol, this.Settings.TickRate);
            IPAddress ip;
            if (!this.ResolveIP(hostname, out ip))
                return;
            this.Transport.Start(ip, port);
        }

        public void Start(ushort port) => this.Start("127.0.0.1", port);

        private void OnNetworkLog(object sender, NetworkLogArgs e)
        {
            Console.Write($"[{e.Time.ToString("HH:mm:ss")}]");
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
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
            }
            Console.Write($"[{e.Priority}]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{e.Message}\n");
        }

        private void OnRevieceMessage(object sender, Packet[] packets)
        {
            foreach(Packet packet in packets)
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
    }
}
