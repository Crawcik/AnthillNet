using System;
using System.Collections.Generic;
using System.Diagnostics;
using AnthillNet.API;

namespace AnthillNet
{
    public class Server : IDisposable
    {
        public ProtocolsType ProtocolType { private set; get; }
        private Base Transport;
        public IDictionary<uint, Connection> Connections { private set; get; } = new Dictionary<uint, Connection>() ;
        public NetworkLog Logging { private set; get; } = new NetworkLog();

        private uint AvalibleID;

        private Server() { }
        public Server(ProtocolsType type)
        {
            Logging.LogName = "Server";
            ProtocolType = type;
            switch (type)
            {
                case ProtocolsType.TCP:
                    Transport = new ServerTCP();
                    break;
                default:
                    Transport = new ServerTCP();
                    break;
            }
        }

        public void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            Transport.TickRate = tickRate;
            Transport.OnConnectionStabilized += OnConnectionStabilized;
            Transport.OnTick += OnTick;
            AvalibleID = 0;
            if (Connections.Count != 0)
                Connections.Clear();
        }



        private void OnTick()
        {
            foreach(Connection connection in Connections.Values)
            {
                if (connection.MaxMessagesCount < connection.Messages.Count)
                {
                    //DDoS protection or something
                }
            }
        }

        public void Start(ushort port) {
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolsType), ProtocolType)}", LogType.Debug);
            Transport.Start("127.0.0.1", port);
        }

        public void Stop()
        {
            foreach (Connection connection in Connections.Values)
                connection.socket.Disconnect(false);
            Transport.Stop();
            Connections.Clear();
        }

        private void OnConnectionStabilized(Connection connection)
        {
            Connections.Add(AvalibleID++, connection);
            Logging.Log($"Client {connection.socket.RemoteEndPoint} connected", LogType.Debug);
        }

        public void Dispose()
        {
            Transport.ForceStop();
            Connections.Clear();
            AvalibleID = 0;
        }
    }
}
