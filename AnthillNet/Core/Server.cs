using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace AnthillNet.Core
{
    public sealed class Server : Host
    {
        private Dictionary<EndPoint, Connection> Dictionary;
        public Server() => this.Logging.LogName = "Server";

        private EndPoint lastEndPoint;


        public override void Start(string hostname, ushort port)
        {
            this.Dictionary = new Dictionary<EndPoint, Connection>();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(hostname), port);
            HostSocket.Bind(endPoint);
            if (Protocol == ProtocolType.TCP)
            {
                HostSocket.Listen(100);
                this.HostSocket.BeginAccept(WaitForConnection, null);
            }
            else if(Protocol == ProtocolType.UDP)
            {
                lastEndPoint = new IPEndPoint(IPAddress.Any, port);
                Connection stack = new Connection(endPoint);
                stack.TempBuffer = new byte[MaxMessageSize];
                HostSocket.BeginReceiveFrom(stack.TempBuffer, 0, MaxMessageSize, 0, ref lastEndPoint, WaitForMessageFrom, stack);
            }
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), Protocol)} protocol", LogType.Debug);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
        }

        private void OnStopped(object sender)
        {
            Logging.Log($"Stopped.", LogType.Info);
            this.Dictionary.Clear();
            base.OnStop -= OnStopped;
            this.HostSocket.Dispose();
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
            if (Protocol == ProtocolType.TCP)
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

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                Socket client = this.HostSocket.EndAccept(ar);
                Connection connection = new Connection(client);
                connection.TempBuffer = new byte[MaxMessageSize];
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                client.BeginReceive(connection.TempBuffer, 0, MaxMessageSize, 0, WaitForMessage, connection);
                this.Logging.Log($"Client {client.RemoteEndPoint} connected", LogType.Info);
                base.Connect(connection);
                this.HostSocket.BeginAccept(WaitForConnection, null);
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
                connection.Socket.BeginReceive(connection.TempBuffer, 0, MaxMessageSize, 0, WaitForMessage, connection);
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
                HostSocket.EndReceiveFrom(ar, ref lastEndPoint);
                if (!this.Dictionary.ContainsKey(lastEndPoint))
                {
                    this.Dictionary.Add(lastEndPoint, new Connection(lastEndPoint as IPEndPoint));
                    this.Logging.Log($"Client {lastEndPoint} connected", LogType.Info);
                }
                connection.Add(Message.Deserialize(connection.TempBuffer));
                HostSocket.BeginReceiveFrom(connection.TempBuffer, 0, MaxMessageSize, 0, ref lastEndPoint, WaitForMessageFrom, connection);
            }
            catch (SocketException)
            {
                HostSocket.Close();
            }
            catch(ObjectDisposedException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            if (this.MaxMessageSize > buf.Length)
                SendOperation(buf, IPAddress);
            else
                base.InternalHostErrorInvoke(new Exception("Message data is too big!"));

        }

        private void SendOperation(byte[] buf, IPEndPoint IPAddress)
        {
            try
            {
                if (Protocol == ProtocolType.TCP)
                {
                    Socket socket = this.Dictionary[IPAddress].Socket;
                    socket.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => socket.EndSend(ar), null);
                }
                else if (Protocol == ProtocolType.UDP)
                {
                    HostSocket.BeginSendTo(buf, 0, buf.Length, 0, IPAddress, (IAsyncResult ar) => HostSocket.EndSend(ar), null);
                }
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        public void SendToAll(Message message)
        {
            byte[] buf = message.Serialize();
            if (this.MaxMessageSize > buf.Length)
                foreach (Connection ip in Dictionary.Values)
                    SendOperation(buf, ip.EndPoint);
            else
                InternalHostErrorInvoke(new Exception("Message data is too big!"));
        }

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.ForceStop();
            base.Dispose();
        }
    }
}
