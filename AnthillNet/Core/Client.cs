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
            if(Protocol == ProtocolType.UDP)
                HostSocket.EnableBroadcast = true;
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
            connection = new Connection();
            base.Disconnect(connection);
        }

        public override void Tick()
        {
            if (this.connection.EndPoint != null)
                if (this.connection.MessagesCount > 0)
                {
                    base.IncomingMessagesInvoke(this.connection);
                    this.connection.ClearMessages();
                }
            base.Tick();
        }
        private void OnStopped(object sender)
        {
            Logging.Log($"Stopped", LogType.Info);
            base.OnStop -= OnStopped;
            this.HostSocket.Dispose();
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                this.HostSocket.EndConnect(ar);
                this.connection = new Connection(HostSocket);
                this.connection.TempBuffer = new byte[MaxMessageSize];
                this.HostSocket.BeginReceive(connection.TempBuffer, 0, MaxMessageSize, 0, WaitForMessage, null);
                this.Logging.Log("Connected to " + ServerEP);
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
                this.HostSocket.EndReceive(ar);
                this.connection.Add(Message.Deserialize(connection.TempBuffer));
                this.HostSocket.BeginReceive(connection.TempBuffer, 0, MaxMessageSize, 0, WaitForMessage, null);
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
                this.Disconnect(connection);
            }
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            try
            {
                byte[] buf = message.Serialize();
                if (this.MaxMessageSize > buf.Length)
                    this.HostSocket.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => this.HostSocket.EndSend(ar), null);
                else
                    base.InternalHostErrorInvoke(new Exception("Message data is too big!"));
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.ForceStop();
            base.Dispose();
        }
    }
}
