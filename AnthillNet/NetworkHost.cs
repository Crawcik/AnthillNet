using AnthillNet.Core;
using AnthillNet.Events;

using System;
using System.Net;
using System.Collections.Generic;

namespace AnthillNet
{
    public class Host : IDisposable
    {
        #region Properties
        public IReadOnlyDictionary<string, IConnection> Connections { private set; get; }
        public Base Transport { private set; get; }
        public Interpreter Interpreter { private set; get; }
        public Order Order { private set; get; }
        public HostType Type { private set; get; }
        public HostSettings Settings
        {
            set
            {
                if (this.Transport.Active)
                    this.Transport.Logging.Log("Cannot change settings while running!", LogType.Warning);
                else
                    settings = value;
            }
            get => settings;
        }
        #endregion

        #region Fields
        private HostSettings settings = new HostSettings() 
        {
            Name = null,
            Port = 8000,
            MaxConnections = 0,
            MaxDataSize = 4096,
            TickRate = 8,
            DualChannels = true,
            Async = true,
            WriteLogsToConsole = true,
            Protocol = ProtocolType.TCP,
            LogPriority = LogType.Error
        };
        #endregion

        #region Initializers
        private Host() { }
        public Host(HostType type) {
            switch (type)
            {
                case HostType.Server:
                    this.Transport = new Server();
                    break;
                case HostType.Client:
                    this.Transport = new Client();
                    break;
                default:
                    throw new NotImplementedException();
            }
            this.Type = type;
            bool server = this.Type == HostType.Server,
                client = this.Type == HostType.Client;
            this.Interpreter = new Interpreter(server, client);
            this.Interpreter.OnMessageGenerate += Interpreter_OnMessageGenerate;
            this.Order = new Order(this.Interpreter);
            this.Transport.OnReceiveData += OnRevieceMessage;
            this.Connections = new Dictionary<string, IConnection>();
        }
        #endregion

        #region Public methods
        public void Start() => Start(this.Type == HostType.Server ? IPAddress.Any: IPAddress.Loopback);
        public void Start(string hostname)
        {
            IPAddress ip;
            if (this.ResolveIP(hostname, out ip))
                Start(ip);
        }
        public void Start(IPAddress address)
        {
            this.Transport.Init(this.Settings.Protocol, this.Settings.Async, this.Settings.TickRate);

            if (this.Settings.Name != null)
                this.Transport.Logging.LogName = this.Settings.Name;
            if (this.Settings.WriteLogsToConsole)
                this.Transport.Logging.OnNetworkLog += OnNetworkLog;
            else
                this.Transport.Logging.OnNetworkLog -= OnNetworkLog;
            this.Transport.Logging.LogPriority = this.Settings.LogPriority;
            this.Transport.MaxMessageSize = this.Settings.MaxDataSize;
            this.Transport.OnConnect += Transport_OnConnect;
            this.Transport.OnDisconnect += Transport_OnDisconnect;

            this.Transport.Start(address, this.Settings.Port);
        }

        private void Transport_OnDisconnect(object sender, IConnection connection)
        {
            if (this.Type == HostType.Server)
            {
                if ((this.Transport as Server).Connections != null)
                    this.Connections = (this.Transport as Server).Connections;
            }
            else if(this.Type == HostType.Client)
            {
                this.Stop();
            }
        }

        public void Stop() 
        {
            if (this.Transport.Active)
                this.Transport.Stop();
            else
                this.Transport.Logging.Log($"{this.Transport.Logging.LogName} is already stopped", AnthillNet.Core.LogType.Info);
        }
        public void SendTo(Message message, string connection)
        {
            byte[] buf = message.Serialize();
            if (this.Settings.MaxDataSize > buf.Length)
            {
                if (this.Type == HostType.Server)
                {
                    Server server = this.Transport as Server;
                    if (server.Connections.ContainsKey(connection))
                        this.Transport.Send(buf, server.Connections[connection].EndPoint);
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
                    foreach (Connection ip in server.Connections.Values)
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
            this.Transport.Dispose();
        }
        #endregion

        #region Private methods
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
                if (packet.Data.Length > this.Transport.MaxMessageSize)
                    this.Transport.Logging.Log($"Received data from {packet.Connection.EndPoint} is too big!", LogType.Warning);
                try
                {
                    if(this.Type == HostType.Client || this.Connections.ContainsKey(packet.Connection.EndPoint.ToString()))
                        this.Interpreter.ResolveMessage(Message.Deserialize(packet.Data));
                }
                catch
                {
                    this.Transport.Logging.Log($"Failed deserializing message from {packet.Connection.EndPoint}!", LogType.Warning);
                }
            }
        }
        private void Interpreter_OnMessageGenerate(object sender, Message message, object target) => Send(message);

        private void Transport_OnConnect(object sender, IConnection connection)
        {
            if(this.Type == HostType.Server)
            {
                if (this.Connections.Count >= this.Settings.MaxConnections && this.Settings.MaxConnections != 0)
                {
                    this.Transport.Disconnect(connection);
                }
                else
                {
                    var x = (this.Transport as Server).Connections;
                    if (x != null)
                        this.Connections = (this.Transport as Server).Connections;
                } 
            }
        }

        private bool ResolveIP(string hostname, out IPAddress iPAddress)
        {
            this.Transport.Logging.Log("Resolving address "+ hostname, LogType.Debug);
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
