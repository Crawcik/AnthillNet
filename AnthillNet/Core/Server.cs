using System;
using System.Collections.Generic;

namespace AnthillNet.Core
{
    public class Server : Host
    {
        public byte TickRate { get; private set; }
        List<string> Connections = new List<string>();

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
            this.Transport.OnConnect += OnConnectionStabilized;
            this.Transport.OnIncomingMessages += OnIncomingMessages;
        }

        public override void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            TickRate = tickRate;
        }

        public void Start(ushort port) {
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), Protocol)}", LogType.Debug);
            Transport.Start("127.0.0.1", port);
        }

        public override void Stop()
        {
            Logging.Log($"Stopping...", LogType.Debug);
            Transport.Stop();
            Logging.Log($"Stopped");
        }
        #endregion
        #region Events
        private void OnConnectionStabilized(Connection connection)
        {
            this.Logging.Log($"Client {connection.EndPoint} connected", LogType.Debug);
        }

        private void OnIncomingMessages(Connection connection)
        {
            Message[] messages = connection.GetMessagesReceived();
            foreach (Message message in messages)
                this.Logging.Log($"Message from {connection.EndPoint}: {(string)message.data}", LogType.Debug);
            this.IncomingMessagesInvoke(messages);
        }
        #endregion
        #region Functions
        public void Send(ulong destiny, object data)
        {
            foreach (string ip in Connections)
                this.SendTo(destiny, data, ip);
        }

        public void SendTo(ulong destiny, object data, string address)
        {
            this.Transport.Send(new Message(destiny, data), address);
        }
        #endregion
        ~Server()
        {
            this.Logging.Log($"Stopping...", LogType.Debug);
            this.Transport.ForceStop();
            this.Connections.Clear();
        }
    }
}
