using System;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Client : Host
    {
        private Connection connection;
        private IPEndPoint ServerEP;

        public Client() => this.Logging.LogName = "Client";

        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.ServerEP = new IPEndPoint(IPAddress.Parse(hostname), port);
            this.connection = new Connection(this.ServerEP);
            this.HostSocket.BeginConnect(ServerEP, WaitForConnection, null);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
        }
        public override void Stop()
        {
            if (!this.Active) return;
            Logging.Log($"Stopping...", LogType.Debug);
            this.HostSocket.Close();
            base.Stop();
        }
        public override void ForceStop()
        {
            if (!this.Active) return;
            Logging.Log($"Force stopping...", LogType.Debug);
            this.HostSocket.Close();
            base.ForceStop();
        }
        public override void Disconnect(Connection connection)
        {
            Logging.Log($"Disconnected from server", LogType.Info);
            this.HostSocket.Close();
            base.Disconnect(connection);
        }

        protected override void Tick()
        {
            if (this.connection.EndPoint != null)
                if (this.connection.MessagesCount > 0)
                {
                    this.Logging.Log($"Message from {connection.EndPoint}: Count {connection.MessagesCount}", LogType.Debug);
                    base.IncomingMessagesInvoke(this.connection);
                    this.connection.ClearMessages();
                }
            base.Tick();
        }
        private void OnStopped(object sender)
        {
            Logging.Log($"Stopped.", LogType.Info);
            base.OnStop -= OnStopped;
            this.HostSocket.Dispose();
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                HostSocket.EndConnect(ar);
                HostSocket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, null);
                connection = new Connection(HostSocket);
                base.Connect(this.connection);
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            try
            {
                HostSocket.EndReceive(ar);
                byte[] buffer = new byte[this.MaxMessageSize];
                HostSocket.Receive(buffer, buffer.Length, 0);
                this.connection.Add(Message.Deserialize(buffer));
                HostSocket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, null);
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
                this.Disconnect(connection);
            }
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            this.HostSocket.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => this.HostSocket.EndSend(ar), null);
        }

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.ForceStop();
            base.Dispose();
        }
    }
}
