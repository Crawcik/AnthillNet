using System;
using System.Collections.Generic;

namespace AnthillNet.Core
{
    public class Server : Host
    {
        public byte TickRate { get; private set; }

        #region Setting
        private Server() { }
        public Server(ProtocolType type)
        {
            Logging.LogName = "Server";
            switch (type)
            {
                case ProtocolType.TCP:
                    Transport = new ServerTCP();
                    break;
                case ProtocolType.UDP:
                    Transport = new ServerUDP();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            Protocol = type;
            Transport.OnConnect += OnConnectionStabilized;
            Transport.OnIncomingMessages += OnIncomingMessages;
        }

        public override void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            TickRate = tickRate;
        }

        public void Start(ushort port) {
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), Protocol)}", LogType.Debug);
            Transport.Start("127.0.0.1", port, TickRate);
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
            Logging.Log($"Client {connection.EndPoint} connected", LogType.Debug);
        }

        private void OnIncomingMessages(Connection connection)
        {
            foreach (Message message in connection.GetMessages())
                Logging.Log($"Message from {connection.EndPoint}: {(string)message.data}", LogType.Debug);
        }
        #endregion
        #region Functions
        public void Send(ulong destiny, object data)
        {
            /*foreach(Connection connection in Connections.Values)
                connection.SendMessage(destiny, data);*/
        }

        public void SendTo(string address, ulong destiny ,object data)
        {
            /*Connections[address].SendMessage(destiny, data);*/
        }
        #endregion
        ~Server()
        {
            /*Logging.Log($"Stopping...", LogType.Debug);
            foreach (Connection connection in Connections.Values)
                connection.socket.Disconnect(false);
            Transport.ForceStop();
            Connections.Clear();*/
        }
    }
}
