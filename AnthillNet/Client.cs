using System;
using System.Threading;
using AnthillNet.API;

namespace AnthillNet
{
    public class Client
    {
        private readonly Base Transport;
        private Connection Host;
        private Client() { }

        public Client(ProtocolsType type)
        {
            switch (type)
            {
                case ProtocolsType.TCP:
                    Transport = new ClientTCP();
                    break;
                default:
                    throw new Exception("Valid protocol type");
            }
        }

        public void Init()
        {
            Init(32);
        }
        public void Init(byte tickRate)
        {
            Transport.TickRate = tickRate;
            Transport.OnConnectionStabilized += OnConnectionStabilized;
        }

        private void OnConnectionStabilized(Connection connection)
        {
            Host = connection;
            Console.WriteLine($"[Client] Connected to: {connection.socket.RemoteEndPoint}");
        }

        public void Connect(string address ,ushort port)
        {
            Transport.Start(address, port);
        }
    }
}
