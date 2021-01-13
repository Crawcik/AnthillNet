using System;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Client : Base
    {
        private Connection connection;
        private IPEndPoint ServerEP;

        public Client() => this.Logging.LogName = "Client";

        #region Public methods
        public override void Start(IPAddress ip, ushort port, bool run_clock = true)
        {
            if (this.Active) return;
            base.Start(ip, port, run_clock);
            if(this.Protocol == ProtocolType.UDP)
                this.HostSocket.EnableBroadcast = true;
            this.ServerEP = new IPEndPoint(ip, port);
            this.connection = new Connection(this.ServerEP);
            if (base.Async)
                this.StartAsync();
            else
                this.StartSync();
        }

        public override void Stop()
        {
            if (!this.Active) return;
            this.Logging.Log($"Stopping...", LogType.Debug);
            this.HostSocket.Close();
            base.Stop();
        }

        public override void ForceStop()
        {
            if (!this.Active) return;
            this.Logging.Log($"Force stopping...", LogType.Debug);
            this.HostSocket.Close();
            base.ForceStop();
        }

        public override void Disconnect(Connection connection)
        {
            if (this.HostSocket != null)
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

        public override void Send(byte[] buffer, IPEndPoint IPAddress)
        {
            try
            {
                if (base.Async)
                    this.HostSocket.BeginSend(buffer, 0, buffer.Length, 0, (IAsyncResult ar) => this.HostSocket.EndSend(ar), null);
                else
                    this.HostSocket.Send(buffer, 0, buffer.Length, 0);
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            if(this.Active)
                this.ForceStop();
            base.Dispose();
        }
        #endregion

        #region Private methods
        private void ClientConnected()
        {
            this.connection = new Connection(this.HostSocket);
            this.connection.tempBuffer = new byte[this.MaxMessageSize];
            this.HostSocket.BeginReceive(connection.tempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, null);
            base.Connect(connection);
            this.Logging.Log("Connected to " + this.ServerEP);
        }
        #endregion

        #region Async
        private void StartAsync()
        {
            this.HostSocket.BeginConnect(this.ServerEP, this.WaitForConnection, null);
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                this.HostSocket.EndConnect(ar);
                this.ClientConnected();
            }
            catch (SocketException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            try
            {
                this.HostSocket.EndReceive(ar);
                this.connection.Add(connection.tempBuffer);
                connection.tempBuffer = new byte[this.MaxMessageSize];
                this.HostSocket.BeginReceive(connection.tempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, null);
            }
            catch (SocketException)
            {
                this.Logging.Log($"{connection.EndPoint} closed connection ", LogType.Warning);
                this.Disconnect(connection);
            }
            catch (ObjectDisposedException)
            {
                this.Logging.Log($"Disconnected from {connection.EndPoint}", LogType.Debug);
                this.Disconnect(connection);
            }
        }
        #endregion

        #region Sync
        private void StartSync()
        {
            try
            {
                this.HostSocket.Connect(this.ServerEP);
                this.ClientConnected();
            }
            catch (Exception e)
            {
                if (e is SocketException)
                    base.InternalHostErrorInvoke(e);
            }
        }
        #endregion
    }
}
