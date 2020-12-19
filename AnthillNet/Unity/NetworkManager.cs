﻿ using AnthillNet;
using AnthillNet.Core;
using AnthillNet.Events;
using AnthillNet.Events.Entities;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

namespace AnthillNet.Unity
{
    public class NetworkManager : MonoBehaviour
    {

        #region Properties
        public static NetworkManager Instance { private set; get; }
        public Base Transport { private set; get; }
        public Interpreter Interpreter { private set; get; }
        public Order Order { private set; get; }
        public EventManager EventManager { private set; get; }
        public HostSettings Settings { private set; get; }
        public HostType HostType { private set; get; }
        #endregion

        #region Fields
        public string hostname = "localhost";
        public ushort port = 7777;
        public HostType hostType;
        public HostSettings settings;
        public UnityEvent OnStart;

        private bool isRunning;
        private float tickTimeElapsed;
        #endregion

        void Awake()
        {
            if (Instance != null)
                return;
            Instance = this;
            DontDestroyOnLoad(this);
            this.Settings = new HostSettings()
            {
                Name = null,
                MaxConnections = 20,
                MaxDataSize = 4096,
                TickRate = 14,
                Async = false,
                WriteLogsToConsole = true,
                Protocol = ProtocolType.UDP,
                LogPriority = AnthillNet.Core.LogType.Warning
            };
        }

        #region Public methods
        public void Run()
        {
            this.HostType = hostType;
            if (this.HostType == HostType.Server)
                this.Transport = new Server();
            else if (this.HostType == HostType.Client)
                this.Transport = new Client();

            bool server = this.HostType == HostType.Server,
                client = this.HostType == HostType.Client;
            this.Interpreter = new Interpreter(server, client);
            this.Order = new Order(this.Interpreter);
            this.EventManager = new EventManager(this.Interpreter);
            this.Transport.OnStop += OnStopped;
            this.Transport.OnReceiveData += OnRevieceMessage;
            this.Interpreter.OnEventIncoming += Interpreter_OnEventIncoming;
            this.Interpreter.OnMessageGenerate += Interpreter_OnMessageGenerate;

            this.Transport.Logging.LogPriority = this.Settings.LogPriority;
            if (this.Settings.Name != null)
                this.Transport.Logging.LogName = this.Settings.Name;
            if (this.Settings.WriteLogsToConsole)
                this.Transport.Logging.OnNetworkLog += OnNetworkLog;
            else
                this.Transport.Logging.OnNetworkLog -= OnNetworkLog;

            this.Transport.MaxMessageSize = this.Settings.MaxDataSize;
            this.Transport.Init(this.Settings.Protocol, this.Settings.TickRate);
            this.Transport.Async = this.Settings.Async;
            IPAddress ip;
            if (!this.ResolveIP(hostname, out ip))
                return;
            this.Transport.Start(ip, port, run_clock: false);
            tickTimeElapsed = 0f;
            isRunning = true;
            this.OnStart?.Invoke();
            if (HostType == HostType.Client)
                this.Send(new Message(0, "CONNECTION TEST"));
        }
        public void Stop()
        {
            isRunning = false;
            if (this.Transport.Active)
                this.Transport.Stop();
            else
                this.Transport.Logging.Log($"{this.Transport.Logging.LogName} is already stopped", AnthillNet.Core.LogType.Info);

        }
        public void Dispose()
        {
            isRunning = false;
            if (this.Transport.Active)
                this.Transport.ForceStop();
            this.Transport.Dispose();
            this.Transport = null;
        }
        #endregion

        #region Private methods
        private void Update()
        {
            if (!isRunning)
                return;
            if (Time.time > tickTimeElapsed)
            {
                float time_before = Time.time;
                this.Transport.Tick();
                tickTimeElapsed = time_before + (1f / this.Settings.TickRate);
            }
        }
        private void OnStopped(object sender)
        {
            isRunning = false;
            this.Transport.Logging.Log($"Stopped", AnthillNet.Core.LogType.Info);
            this.Transport.OnStop -= OnStopped;
            this.Transport.Dispose();
        }
        private void OnNetworkLog(object sender, NetworkLogArgs e)
        {
            string text = $"[{e.LogName}][{e.Priority}] {e.Message}";
            switch (e.Priority)
            {
                case AnthillNet.Core.LogType.Info:
                    Debug.Log(text);
                    break;
                case AnthillNet.Core.LogType.Error:
                    Debug.LogError(text);
                    break;
                case AnthillNet.Core.LogType.Warning:
                    Debug.LogWarning(text);
                    break;
                case AnthillNet.Core.LogType.Debug:
                    Debug.Log(text);
                    break;
            }
        }
        private void OnRevieceMessage(object sender, Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                if (packet.data.Length > this.Transport.MaxMessageSize)
                    this.Transport.Logging.Log($"Received data from {packet.connection.EndPoint} is too big!", AnthillNet.Core.LogType.Warning);
                try
                {
                    this.Interpreter.ResolveMessage(Message.Deserialize(packet.data));
                }
                catch(Exception e)
                {
                    this.Transport.Logging.Log($"Failed deserializing message from {packet.connection.EndPoint}!", AnthillNet.Core.LogType.Warning);
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
                if (this.hostType == HostType.Server)
                {
                    Server server = this.Transport as Server;
                    if (server.Dictionary.ContainsKey(connection))
                        this.Transport.Send(buf, server.Dictionary[connection].EndPoint);
                    else
                        this.Transport.Logging.Log($"Client {connection} isn't connected!", AnthillNet.Core.LogType.Warning);

                }
                else if (this.hostType == HostType.Client)
                {
                    this.Transport.Send(buf, null);
                }
            }
            else this.Transport.Logging.Log("Message data is too big!", AnthillNet.Core.LogType.Error);
        }
        private void Send(Message message)
        {
            byte[] buf = message.Serialize();
            if (this.Settings.MaxDataSize > buf.Length)
            {
                if (this.hostType == HostType.Server)
                {
                    Server server = this.Transport as Server;
                    foreach (Connection ip in server.Dictionary.Values)
                        this.Transport.Send(buf, ip.EndPoint);

                }
                else if (this.hostType == HostType.Client)
                {
                    this.Transport.Send(buf, null);
                }
            }
            else this.Transport.Logging.Log("Message data is too big!", AnthillNet.Core.LogType.Error);
        }
        private void Interpreter_OnEventIncoming(object sender, Message message, string target)
        {
            EventCommand command = (EventCommand)message.data;
            EventManager.HandleEvent(command.args, command.type);
        }
        #endregion
    }
}