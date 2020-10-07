using System;
using System.Collections.Generic;
using System.Net;

namespace AnthillNet.Core
{
    public sealed class Server : Host
    {
        public byte TickRate { get; private set; }
        List<Connection> Connections = new List<Connection>();

        #region Setting
        private Server() { }
        public Server(ProtocolType type)
        {
            this.Logging.LogName = "Server";
            switch (type)
            {
                case ProtocolType.TCP:
                    this.Transport = new ServerTCP();
                    break;
                case ProtocolType.UDP:
                    this.Transport = new ServerUDP();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            this.Protocol = type;
        }

        public override void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            TickRate = tickRate;
            base.Init(tickRate);
        }

        public void Start(ushort port) {
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), Protocol)}", LogType.Debug);
            Transport.Start("127.0.0.1", port);
        }

        public override void Stop(Message[] additional_packages = null)
        {
            Logging.Log($"Stopping...", LogType.Debug);
            Transport.Stop();
            base.Stop(additional_packages);
        }

        #endregion

        #region Events
        protected override void OnHostConnect(Connection connection)
        {
            this.Logging.Log($"Client {connection.EndPoint} connected", LogType.Info);
            this.Connections.Add(connection);
            base.OnHostConnect(connection);
        }

        protected override void OnHostDisconnect(Connection connection)
        {
            this.Logging.Log($"Client {connection.EndPoint} disconnect from server", LogType.Info);
            this.Connections.Remove(connection);
            base.OnHostDisconnect(connection);
        }

        protected override void OnIncomingMessages(Connection connection)
        {
            Message[] messages = connection.GetMessages();
            foreach (Message message in messages)
                this.Logging.Log($"Message from {connection.EndPoint}: {(string)message.data}", LogType.Debug);
            base.OnIncomingMessages(connection);
        }
        #endregion

        #region Functions
        public void Send(ulong destiny, object data)
        {
            foreach (Connection ip in Connections)
                this.SendTo(destiny, data, ip.EndPoint);
        }

        public void SendTo(ulong destiny, object data, IPEndPoint address)
        {
            this.Transport.Send(new Message(destiny, data), address);
        }
        #endregion
        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.Logging.Log($"Force stopping...", LogType.Info);
            this.Transport.ForceStop();
            this.Transport = null;
            this.Connections.Clear();
        }
    }
}
