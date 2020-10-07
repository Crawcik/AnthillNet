using System;

namespace AnthillNet.Core
{
    public sealed class Client : Host
    {
        public Connection Host { get; private set; }
        public byte TickRate { get; private set; }
        public string HostAddress => this.Host.EndPoint.ToString();

        #region Setting
        private Client() { }

        public Client(ProtocolType type)
        {
            Host = new Connection();
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
        }

        public override void Init(byte tickRate = 32)
        {
            this.Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            this.TickRate = tickRate;
            base.Init(tickRate);
        }

        public override void Stop(Message[] additional_packages = null)
        {
            Logging.Log($"Stopping...", LogType.Debug);
            if (additional_packages != null)
                foreach (Message message in additional_packages)
                    Transport.Send(message, Host.EndPoint);
            Transport.Stop();
            base.Stop();
        }
        #endregion

        #region Events
        protected override void OnHostConnect(Connection connection)
        {
            Host = connection;
            base.OnHostConnect(connection);
        }
        protected override void OnHostDisconnect(Connection connection)
        {
            base.OnHostDisconnect(connection);
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
            this.Transport.Send(new Message(destiny, data), Host.EndPoint);
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
