using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using AnthillNet.API;

namespace AnthillNet
{
    public class Server : Host, IDisposable
    {
        private IDictionary<string, Connection> Connections = new Dictionary<string, Connection>() ;
        public NetworkLog Logging { private set; get; } = new NetworkLog();

        private Server() { }
        public Server(ProtocolsType type)
        {
            Logging.LogName = "Server";
            switch (type)
            {
                case ProtocolsType.TCP:
                    Transport = new ServerTCP();
                    break;
                default:
                    throw new Exception("Valid protocol type");
            }
            Protocol = type;
            Transport.OnConnectionStabilized += OnConnectionStabilized;
            Transport.OnTick += OnTick;
        }

        public override void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            Transport.TickRate = tickRate;
            if (Connections.Count != 0)
                Connections.Clear();
        }



        private void OnTick()
        {
            foreach(KeyValuePair<string, Connection> connection in Connections)
            {
                if(!connection.Value.socket.Connected)
                {
                    Logging.Log($"Connection lost with {connection.Key}!", LogType.Error);
                    continue;
                }    
                if (connection.Value.MaxMessagesCount < connection.Value.Messages.Count)
                {
                    Logging.Log($"{connection.Key} sends too many data!", LogType.Debug);
                }
                if (connection.Value.Messages.Count != 0)
                    this.IncomingMessagesInvoke(connection.Value.Messages.ToArray());
            }
        }

        public void Start(ushort port) {
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolsType), Protocol)}", LogType.Debug);
            Transport.Start("127.0.0.1", port);
        }

        public override void Stop()
        {
            Logging.Log($"Stopping...", LogType.Debug);
            foreach (Connection connection in Connections.Values)
                connection.socket.Disconnect(false);
            Connections.Clear();
            Transport.Stop();
            Logging.Log($"Stopped");
        }

        public void Send(string address, ulong destiny ,object data)
        {
            Connections[address].SendMessage(destiny, data);
        }

        private void OnConnectionStabilized(Connection connection)
        {
            Connections.Add(connection.socket.RemoteEndPoint.ToString(), connection);
            Logging.Log($"Client {connection.socket.RemoteEndPoint} connected", LogType.Debug);
        }

        public void Dispose()
        {
            Logging.Log($"Stopping...", LogType.Debug);
            foreach (Connection connection in Connections.Values)
                connection.socket.Disconnect(false);
            Transport.ForceStop();
            Connections.Clear();
        }
    }
}
