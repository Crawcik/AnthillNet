using System;

namespace AnthillNet.Core
{
    public class Client : Host
    {
        private Connection Host;
        public byte TickRate { get; private set; }
        public string HostAddress => this.Host.EndPoint.ToString();

        #region Setting
        private Client() { }

        public Client(ProtocolType type)
        {
            this.Logging.LogName = "Client";
            switch (type)
            {
                case ProtocolType.TCP:
                    this.Transport = new ClientTCP();
                    break;
                case ProtocolType.UDP:
                    this.Transport = new ClientUDP();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            this.Protocol = type;
            this.Transport.OnConnect += OnConnectionStabilized;
            this.Transport.OnIncomingMessages += OnIncomingMessages;
            this.Transport.OnInternalHostError += OnHostError;
        }

        public override void Init(byte tickRate = 32)
        {
            this.Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            this.TickRate = tickRate;
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
            this.Host = connection;
            this.Logging.Log($"Connected to: {connection.EndPoint}");
        }

        private void OnIncomingMessages(Connection connection)
        {
            
        }
        private void OnHostError(Exception exception)
        {
            this.Logging.Log("Internal error occured:", LogType.Error);
            this.Logging.Log(exception.ToString(), LogType.Error);
        }
        #endregion
        #region Functions
        public void Connect(string address ,ushort port)
        {
            this.Logging.Log($"Connecting to: {address}", LogType.Debug);
            this.Transport.Start(address, port);
        }

        public void Send(ulong destiny, object data)
        {
            this.Transport.Send(new Message(0, data), HostAddress);
        }
        #endregion

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.Logging.Log($"Force stopping...", LogType.Info);
            this.Transport.ForceStop();
            this.Transport = null;
        }
    }
}
