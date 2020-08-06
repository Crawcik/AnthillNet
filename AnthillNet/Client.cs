using System;
using AnthillNet.API;

namespace AnthillNet
{
    public class Client : Host, IDisposable
    {
        private Connection Host;
        public NetworkLog Logging = new NetworkLog();
        public bool Connected => Host.socket.Connected;
        public string HostAddress => Host.socket.RemoteEndPoint.ToString();

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
            Transport.OnTick += OnTick;
            Transport.OnConnectionStabilized += OnConnectionStabilized;
        }

        public override void Init(byte tickRate = 32)
        {
            Logging.Log($"Start initializing with {tickRate} tick rate", LogType.Debug);
            Transport.TickRate = tickRate;
        }

        private void OnTick()
        {
            if(!Host.socket.Connected)
                Logging.Log($"Connection lost!", LogType.Error);
            if (Host.Messages.Count != 0)
                this.IncomingMessagesInvoke(Host.Messages.ToArray());
        }

        private void OnConnectionStabilized(Connection connection)
        {
            Host = connection;
            this.ConnectedInvoke(connection.socket.RemoteEndPoint.ToString());
            Logging.Log($"Connected to: {connection.socket.RemoteEndPoint}");
        }

        public void Connect(string address ,ushort port)
        {
            Logging.Log($"Connecting to: {address}", LogType.Debug);
            Transport.Start(address, port);
        }

        public void Send(ulong destiny, object data)
        {
            Host.SendMessage(destiny, data);
        }

        public override void Stop()
        {
            Logging.Log($"Stopping...", LogType.Debug);
            Host.socket.Disconnect(false);
            Host = null;
            Transport.Stop();
            Logging.Log($"Stopped");
        }

        public void Dispose()
        {
            Logging.Log($"Disposing", LogType.Debug);
            Host.socket.Disconnect(false);
            Transport.ForceStop();
            Host = null;
        }
    }
}
