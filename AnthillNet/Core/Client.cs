using System;

namespace AnthillNet.Core
{
    public class Client : Host
    {
        private Connection Host;
        public byte TickRate { get; private set; }
        public string HostAddress => Host.EndPoint.ToString();

        #region Setting
        private Client() { }

        public Client(ProtocolType type)
        {
            Logging.LogName = "Client";
            switch (type)
            {
                case ProtocolType.TCP:
                    Transport = new ClientTCP();
                    break;
                case ProtocolType.UDP:
                    Transport = new ClientUDP();
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
            Host = connection;
            Logging.Log($"Connected to: {connection.EndPoint}");
        }

        private void OnIncomingMessages(Connection connection)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Functions
        public void Connect(string address ,ushort port)
        {
            Logging.Log($"Connecting to: {address}", LogType.Debug);
            Transport.Start(address, port, TickRate);
        }

        public void Send(ulong destiny, object data)
        {
            Transport.Send(new Message(0, data));
        }
        #endregion
        ~Client()
        {
            Logging.Log($"Disposing", LogType.Debug);
            Transport.ForceStop();
        }
    }
}
