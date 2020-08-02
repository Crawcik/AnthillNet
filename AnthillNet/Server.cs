using System;
using System.Collections.Generic;
using AnthillNet.API;

namespace AnthillNet
{
    public class Server
    {
        private uint AvalibleID;
        private readonly Base Transport;
        public readonly IDictionary<uint, Connection> Connections = new Dictionary<uint, Connection>();
        private Server() { }
        public Server(ProtocolsType type)
        {
            switch (type)
            {
                case ProtocolsType.TCP:
                    Transport = new ServerTCP();
                    break;
                default:
                    throw new Exception("Valid protocol type");
            }
        }
        public void Init() => Init(32);
        public void Init(byte tickRate)
        {
            Transport.TickRate = tickRate;
            Transport.OnConnectionStabilized += OnConnectionStabilized;
            AvalibleID = 0;
            if (Connections.Count != 0)
                Connections.Clear();
        }

        public void Start(ushort port) => Transport.Start("127.0.0.1", port);

        private void OnConnectionStabilized(Connection connection)
        {
            Connections.Add(AvalibleID++, connection);
            Console.WriteLine("[Server] Connected");
        }
    }
}
