using System;

namespace AnthillNet.Core
{
    public class Client : Host, IDisposable
    {
        private Connection Host;
        private byte TickRate;
        public string HostAddress => Host.EndPoint.ToString();

        private Client() { }

        public Client(ProtocolType type)
        {
            Logging.LogName = "Client";
            switch (type)
            {
                case ProtocolType.TCP:
                    Transport = new ClientTCP();
                    break;
                default:
                    throw new Exception("Valid protocol type");
            }
            Transport.OnTick += OnTick;
            Transport.OnConnect += OnConnectionStabilized;
            Transport.OnIncomingMessages += OnIncomingMessages;
        }

        public override void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            TickRate = tickRate;
        }

        private void OnTick()
        {
            Send(0, "BRUH MOMENTO");
        }

        private void OnConnectionStabilized(Connection connection)
        {
            Host = connection;
            Logging.Log($"Connected to: {connection.EndPoint}");
        }

        private void OnIncomingMessages(Connection connection)
        {
            throw new NotImplementedException();
        }

        public void Connect(string address ,ushort port)
        {
            Logging.Log($"Connecting to: {address}", LogType.Debug);
            Transport.Start(address, port);
        }

        public void Send(ulong destiny, object data)
        {
            Transport.Send(new Message(0, data));
        }

        public override void Stop()
        {
            Logging.Log($"Stopping...", LogType.Debug);
            Transport.Stop();
            Logging.Log($"Stopped");
        }

        public void Dispose()
        {
            Logging.Log($"Disposing", LogType.Debug);
            Transport.ForceStop();
        }
    }
}
