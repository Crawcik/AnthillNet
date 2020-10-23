using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Server : Host
    {
        private Dictionary<EndPoint, Connection> Dictionary;
        private EndPoint LastEndPoint;

        public Server() => this.Logging.LogName = "Server";

        #region Public methods
        public override void Start(string hostname, ushort port)
        {
            this.Dictionary = new Dictionary<EndPoint, Connection>();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(hostname), port);
            this.HostSocket.Bind(endPoint);
            if (this.Protocol == ProtocolType.TCP)
            {
                this.HostSocket.Listen(100);
                this.HostSocket.BeginAccept(this.WaitForConnection, null);
            }
            else if(this.Protocol == ProtocolType.UDP)
            {
                this.LastEndPoint = new IPEndPoint(IPAddress.Any, port);
                Connection stack = new Connection(endPoint){ TempBuffer = new byte[this.MaxMessageSize] };
                this.HostSocket.BeginReceiveFrom(stack.TempBuffer, 0, this.MaxMessageSize, 0, ref this.LastEndPoint, WaitForMessageFrom, stack);
            }
            this.Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), this.Protocol)} protocol", LogType.Debug);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
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
            if (this.Protocol == ProtocolType.TCP)
            {
                connection.Socket.Shutdown(SocketShutdown.Both);
                connection.Socket.Close();
            }
            this.Dictionary.Remove(connection.EndPoint);
            this.Logging.Log($"Client {connection.EndPoint} disconnected", LogType.Info);
            connection = new Connection();
            base.Disconnect(connection);
        }
        public override void Tick()
        {
            foreach (Connection connection in this.Dictionary.Values)
                if (connection.MessagesCount > 0)
                {
                    base.IncomingMessagesInvoke(connection);
                    connection.ClearMessages();
                }
            base.Tick();
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            if (this.MaxMessageSize > buf.Length)
                this.SendOperation(buf, IPAddress);
            else
                base.InternalHostErrorInvoke(new Exception("Message data is too big!"));

        }
        public void SendToAll(Message message)
        {
            byte[] buf = message.Serialize();
            if (this.MaxMessageSize > buf.Length)
                foreach (Connection ip in this.Dictionary.Values)
                    this.SendOperation(buf, ip.EndPoint);
            else
                this.InternalHostErrorInvoke(new Exception("Message data is too big!"));
        }
        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.ForceStop();
            base.Dispose();
        }
        #endregion

        #region Private methods
        private void OnStopped(object sender)
        {
            this.Logging.Log($"Stopped.", LogType.Info);
            this.Dictionary.Clear();
            base.OnStop -= OnStopped;
            this.HostSocket.Dispose();
        }
        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                Socket client = this.HostSocket.EndAccept(ar);
                Connection connection = new Connection(client);
                connection.TempBuffer = new byte[this.MaxMessageSize];
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                client.BeginReceive(connection.TempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, connection);
                this.Logging.Log($"Client {client.RemoteEndPoint} connected", LogType.Info);
                base.Connect(connection);
                this.HostSocket.BeginAccept(this.WaitForConnection, null);
            }
            catch (Exception e)
            {
                if (e is SocketException)
                    base.InternalHostErrorInvoke(e);
            }
        }
        private void WaitForMessage(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            try
            {
                connection.Socket.EndReceive(ar);
                connection.Add(Message.Deserialize(connection.TempBuffer));
                connection.Socket.BeginReceive(connection.TempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, connection);
            }
            catch (SocketException)
            {
                this.Logging.Log($"Client {connection.Socket.RemoteEndPoint} disconnected", LogType.Info);
                this.Dictionary.Remove(connection.Socket.RemoteEndPoint);
                this.Disconnect(connection);
            }
            catch (ObjectDisposedException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }
        private void WaitForMessageFrom(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            try
            {
                this.HostSocket.EndReceiveFrom(ar, ref this.LastEndPoint);
                if (!this.Dictionary.ContainsKey(this.LastEndPoint))
                {
                    this.Dictionary.Add(this.LastEndPoint, new Connection(this.LastEndPoint as IPEndPoint));
                    this.Logging.Log($"Client {this.LastEndPoint} connected", LogType.Info);
                }
                connection.Add(Message.Deserialize(connection.TempBuffer));
                HostSocket.BeginReceiveFrom(connection.TempBuffer, 0, this.MaxMessageSize, 0, ref this.LastEndPoint, this.WaitForMessageFrom, connection);
            }
            catch (SocketException)
            {
                this.HostSocket.Close();
            }
            catch(ObjectDisposedException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }
        private void SendOperation(byte[] buf, IPEndPoint IPAddress)
        {
            try
            {
                if (this.Protocol == ProtocolType.TCP)
                {
                    Socket socket = this.Dictionary[IPAddress].Socket;
                    socket.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => socket.EndSend(ar), null);
                }
                else if (this.Protocol == ProtocolType.UDP)
                {
                    this.HostSocket.BeginSendTo(buf, 0, buf.Length, 0, IPAddress, (IAsyncResult ar) => this.HostSocket.EndSend(ar), null);
                }
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }
        #endregion
    }
}
