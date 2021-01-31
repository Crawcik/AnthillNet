using AnthillNet.Core;
using AnthillNet.Events;

using System;
using System.Net;
using System.Collections.Generic;

using FlaxEngine;
using LogType = AnthillNet.Core.LogType;

namespace AnthillNet.Flax
{
    public class NetworkManager : Script
    {
        public delegate void NetworkEvent();
        public delegate void NetworkConnect(string connection);

        #region Properties
        public static NetworkManager Instance { private set; get; }
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
        public string hostname = "localhost";
        public ushort port = 7777;
        public HostType hostType;
        private HostSettings settings = new HostSettings()
        {
            Name = null,
            MaxConnections = 20,
            MaxDataSize = 4096,
            TickRate = 16,
            Async = false,
            WriteLogsToConsole = true,
            Protocol = ProtocolType.UDP,
            LogPriority = LogType.Warning
        };
        private bool isRunning;
        private float tickTimeDestination, tickTimeNow;
        #endregion

        #region Events
        public event NetworkEvent OnStartEvent, OnStartAsServerEvent, OnStartAsClientEvent;
        public event NetworkEvent OnStopEvent, OnStopAsServerEvent, OnStopAsClienEventt;
        public event NetworkConnect OnClientConnectEvent, OnClientDisconnectEvent;
        #endregion

        #region Overrides
        public override void OnAwake()
        {
            if (Instance != null)
                return;
            Instance = this;
            tickTimeDestination = 0;
            tickTimeNow = 0;
        }

        public override void OnFixedUpdate()
        {
            if (!isRunning)
                return;
            if (tickTimeNow >= tickTimeDestination)
            {
                this.Transport.Tick();
                tickTimeDestination = tickTimeNow + (1f / this.Settings.TickRate);
            }
            tickTimeNow += Time.DeltaTime;
        }
        #endregion

        #region Public methods
        public virtual void StartHost()
        {
            //Setting and init
            switch (hostType)
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
            this.Type = hostType;
            bool server = this.Type == HostType.Server,
                client = this.Type == HostType.Client;
            this.Interpreter = new Interpreter(server, client);
            this.Interpreter.OnMessageGenerate += Interpreter_OnMessageGenerate;
            this.Order = new Order(this.Interpreter);
            this.Transport.OnReceiveData += OnRevieceMessage;
            this.Connections = new Dictionary<string, IConnection>();

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

            IPAddress ip;
            if (!this.ResolveIP(hostname, out ip))
                return;
            tickTimeNow = 0;
            tickTimeDestination = 0;
            this.Transport.Start(ip, port);
            isRunning = true;

            this.OnStartHost();
            this.OnStartEvent?.Invoke();
            if (this.Type == HostType.Client)
            {
                this.OnStartAsClient();
                this.OnStartAsClientEvent?.Invoke();
                this.Send(new Message(0, "CONNECTION TEST"));
            }
            else if (this.Type == HostType.Server)
            {
                this.OnStartAsServer();
                this.OnStartAsServerEvent?.Invoke();
            }
        }
        public virtual void StopHost()
        {
            isRunning = false;
            if (this.Transport.Active)
                this.Transport.Stop();
            else
                this.Transport.Logging.Log($"{this.Transport.Logging.LogName} is already stopped", AnthillNet.Core.LogType.Info);

        }
        public virtual void Dispose()
        {
            isRunning = false;
            if (this.Transport.Active)
                this.Transport.ForceStop();
            this.Transport.Dispose();
            this.Transport = null;
            Destroy(this);
        }
        #endregion

        #region Private methods
        private void OnStopped(object sender)
        {
            isRunning = false;
            this.OnStopHost();
            this.OnStopEvent?.Invoke();
            if (this.Type == HostType.Client)
            {
                this.OnStopAsClient();
                this.OnStopAsClienEventt?.Invoke();
            }
            else if (this.Type == HostType.Server)
            {
                this.OnStopAsClient();
                this.OnStopAsServerEvent?.Invoke();
            }
            this.Transport.Logging.Log($"Stopped", AnthillNet.Core.LogType.Info);
            this.Transport.OnStop -= OnStopped;
            this.Transport.Dispose();
        }
        private void OnNetworkLog(object sender, NetworkLogArgs e)
        {
            string text = $"[{e.LogName}][{e.Priority}] {e.Message}";
            switch (e.Priority)
            {
                case LogType.Info:
                    Debug.Log(text);
                    break;
                case LogType.Error:
                    Debug.LogError(text);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(text);
                    break;
                case LogType.Debug:
                    Debug.Log(text);
                    break;
            }
        }
        private void OnRevieceMessage(object sender, Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                if (packet.Data.Length > this.Transport.MaxMessageSize)
                    this.Transport.Logging.Log($"Received data from {packet.Connection.EndPoint} is too big!", LogType.Warning);
                try
                {
                    if (this.Type == HostType.Client || this.Connections.ContainsKey(packet.Connection.EndPoint.ToString()))
                        this.Interpreter.ResolveMessage(Message.Deserialize(packet.Data));
                }
                catch
                {
                    this.Transport.Logging.Log($"Failed deserializing message from {packet.Connection.EndPoint}!", LogType.Warning);
                }
            }
        }
        private void Interpreter_OnMessageGenerate(object sender, Message message, string target)
        {
            if (string.IsNullOrEmpty(target))
                Send(message);
            else
                SendTo(message, target);
        }
        private bool ResolveIP(string hostname, out IPAddress iPAddress)
        {
            if (hostname == "localhost")
            {
                iPAddress = IPAddress.Loopback;
                return true;
            }
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
            this.Transport.Logging.Log("Given hostname is invalid!", AnthillNet.Core.LogType.Error);
            iPAddress = null;
            return false;
        }
        private void SendTo(Message message, string connection)
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
        private void Send(Message message)
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
                else if (this.Type == HostType.Client)
                {
                    this.Transport.Send(buf, null);
                }
            }
            else this.Transport.Logging.Log("Message data is too big!", LogType.Error);
        }

        private void Transport_OnConnect(object sender, IConnection connection)
        {
            if (this.Type == HostType.Server)
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
                    this.OnClientConnect(connection.EndPoint.ToString());
                    this.OnClientConnectEvent?.Invoke(connection.EndPoint.ToString());
                }
            }
        }
        private void Transport_OnDisconnect(object sender, IConnection connection)
        {
            if (this.Type == HostType.Server)
            {
                if ((this.Transport as Server).Connections != null)
                    this.Connections = (this.Transport as Server).Connections;
                this.OnClientDisconnect(connection.EndPoint.ToString());
                this.OnClientDisconnectEvent?.Invoke(connection.EndPoint.ToString());
            }
            else if (this.Type == HostType.Client)
            {
                this.StopHost();
            }
        }
        #endregion

        #region Event methods
        public virtual void OnStartHost() { }
        public virtual void OnStartAsServer() { }
        public virtual void OnStartAsClient() { }
        public virtual void OnStopHost() { }
        public virtual void OnStopAsServer() { }
        public virtual void OnStopAsClient() { }
        public virtual void OnClientConnect(string connection) { }
        public virtual void OnClientDisconnect(string connection) { }
        #endregion
    }
}