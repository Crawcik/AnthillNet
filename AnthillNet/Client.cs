using System;
using AnthillNet.API;

namespace AnthillNet
{
    public class Client : IDisposable
    {
        private readonly Base Transport;
        private Connection Host;
        public NetworkLog Logging = new NetworkLog();


        private Client() { }

        public Client(ProtocolsType type)
        {
            Logging.LogName = "Client";
            switch (type)
            {
                case ProtocolsType.TCP:
                    Transport = new ClientTCP();
                    break;
                default:
                    throw new Exception("Valid protocol type");
            }
        }

        public void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            Transport.TickRate = tickRate;
            Transport.OnConnectionStabilized += OnConnectionStabilized;
        }

        private void OnConnectionStabilized(Connection connection)
        {
            Host = connection;
            Logging.Log($"Connected to: {connection.socket.RemoteEndPoint}");
        }

        public void Connect(string address ,ushort port)
        {
            Logging.Log($"Connecting to: {address}", LogType.Debug);
            Transport.Start(address, port);
        }

        public void Dispose()
        {
            Logging.Log($"Disposing", LogType.Debug);
            Host.socket.Disconnect(false);
            Host = null;
        }
    }
}
