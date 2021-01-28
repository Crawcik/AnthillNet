using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Server : Base
    {
        public readonly Dictionary<string, Connection> Dictionary;
        private readonly List<IPEndPoint> BindedEndpoint;
        private EndPoint LastEndPoint;

        public Server()
        {
            this.Dictionary = new Dictionary<string, Connection>();
            this.BindedEndpoint = new List<IPEndPoint>();
        }

        #region Public methods
        public override void Init(ProtocolType protocol, bool async = true, byte tickRate = 32)
        {
            base.Init(protocol, async, tickRate);
            base.Logging.LogName = "Server";
        }

        public override void Start(IPAddress ip, ushort port)
        {

            if(!this.Active)
                base.Start(ip, port);
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            if (!BindedEndpoint.Contains(endPoint))
            {
                BindedEndpoint.Add(endPoint);
                this.HostSocket.Bind(endPoint);
                this.Logging.Log($"{ip}:{port} added to pool", LogType.Debug);
            }
            if (BindedEndpoint.Count == 1)
            {
                if (base.Protocol == ProtocolType.TCP)
                    this.HostSocket.Listen(100);
                else if (base.Protocol == ProtocolType.UDP)
                    this.LastEndPoint = new IPEndPoint(IPAddress.Any, port);
                if (base.Async)
                    this.StartAsync();
                else
                    this.StartSync();
                this.Logging.Log($"Starting listening with {Enum.GetName(typeof(ProtocolType), this.Protocol)} protocol", LogType.Debug);
            }
        }

        public override void Stop()
        {
            if (!this.Active) return;
            this.Logging.Log($"Stopping...", LogType.Debug);
            if(Protocol == ProtocolType.TCP)
                foreach (Connection connection in this.Dictionary.Values)
                    connection.Socket.Close();
            this.Dictionary.Clear();
            this.BindedEndpoint.Clear();
            base.Stop();
        }
        public override void Disconnect(Connection connection)
        {
            if (this.Protocol == ProtocolType.TCP)
            {
                connection.Socket.Shutdown(SocketShutdown.Both);
                connection.Socket.Close();
            }
            this.Dictionary.Remove(connection.EndPoint.ToString());
            this.Logging.Log($"Client {connection.EndPoint} disconnected", LogType.Info);
            connection = new Connection();
            base.Disconnect(connection);
        }
        public override void Tick()
        {
            lock (this.Dictionary)
            {
                if (!this.Async)
                {
                    if (base.Protocol == ProtocolType.UDP)
                    {
                        while (this.HostSocket.Available > 0)
                        {
                            byte[] buffer = new byte[this.MaxMessageSize];
                            this.HostSocket.ReceiveFrom(buffer, 0, buffer.Length, 0, ref LastEndPoint);
                            if (!this.Dictionary.ContainsKey(this.LastEndPoint.ToString()))
                            {
                                this.Dictionary.Add(this.LastEndPoint.ToString(), new Connection(this.LastEndPoint as IPEndPoint));
                                this.Logging.Log($"Client {LastEndPoint} connected", LogType.Info);
                                base.Connect(new Connection(this.LastEndPoint as IPEndPoint));
                            }
                            this.Dictionary[this.LastEndPoint.ToString()].Add(buffer);
                        }
                    }
                }
                foreach (Connection connection in this.Dictionary.Values)
                    if (connection.MessagesCount > 0)
                    {
                        base.IncomingMessagesInvoke(connection);
                        connection.ClearMessages();
                    }

            }
            base.Tick();
        }
        public override void Send(byte[] buffer, IPEndPoint IPAddress)
        {
            try
            {
                if (this.Protocol == ProtocolType.TCP)
                {
                    Socket socket = this.Dictionary[IPAddress.ToString()].Socket;
                    if (this.Async)
                        socket.BeginSend(buffer, 0, buffer.Length, 0, (IAsyncResult ar) => socket.EndSend(ar), null);
                    else
                        socket.Send(buffer, 0, buffer.Length, 0);
                }
                else if (this.Protocol == ProtocolType.UDP)
                {
                    if (this.Async)
                        this.HostSocket.BeginSendTo(buffer, 0, buffer.Length, 0, IPAddress, (IAsyncResult ar) => this.HostSocket.EndSend(ar), null);
                    else
                        this.HostSocket.SendTo(buffer, 0, buffer.Length, 0, IPAddress);
                }
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }

        }
        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.Dictionary.Clear();
            this.BindedEndpoint.Clear();
            if (this.Active)
                base.ForceStop();
            base.Dispose();
        }
        #endregion

        #region Async
        private void StartAsync()
        {
            if (this.Protocol == ProtocolType.TCP)
            {
                this.HostSocket.BeginAccept(this.WaitForConnection, null);
            }
            else if (this.Protocol == ProtocolType.UDP)
            {
                Connection stack = new Connection(LastEndPoint as IPEndPoint) { tempBuffer = new byte[this.MaxMessageSize] };
                this.HostSocket.BeginReceiveFrom(stack.tempBuffer, 0, this.MaxMessageSize, 0, ref this.LastEndPoint, WaitForMessageFrom, stack);
            }
        }
        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {

                Socket client = this.HostSocket.EndAccept(ar);
                Connection connection = new Connection(client);
                connection.tempBuffer = new byte[this.MaxMessageSize];
                this.Dictionary.Add(client.RemoteEndPoint.ToString(), connection);
                client.BeginReceive(connection.tempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, connection);
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
                connection.Add(connection.tempBuffer);
                connection.Socket.BeginReceive(connection.tempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, connection);
            }
            catch (SocketException)
            {
                this.Logging.Log($"Client {connection.EndPoint} disconnected", LogType.Warning);
                this.Dictionary.Remove(connection.EndPoint.ToString());
                this.Disconnect(connection);
            }
            catch (ObjectDisposedException)
            {
                this.Logging.Log($"Server has disconnected {connection.EndPoint} ", LogType.Debug);
            }
        }
        private void WaitForMessageFrom(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            try
            {
                this.HostSocket.EndReceiveFrom(ar, ref this.LastEndPoint);
                if (!this.Dictionary.ContainsKey(this.LastEndPoint.ToString()))
                {
                    this.Dictionary.Add(this.LastEndPoint.ToString(), new Connection(this.LastEndPoint as IPEndPoint));
                    this.Logging.Log($"Client {LastEndPoint} connected", LogType.Info);
                    base.Connect(new Connection((IPEndPoint)this.LastEndPoint));
                }
                this.Dictionary[this.LastEndPoint.ToString()].Add(connection.tempBuffer);
                connection.tempBuffer = new byte[MaxMessageSize];
                this.HostSocket.BeginReceiveFrom(connection.tempBuffer, 0, this.MaxMessageSize, 0, ref this.LastEndPoint, this.WaitForMessageFrom, connection);
            }
            catch (SocketException)
            {
                this.HostSocket.Close();
            }
            catch (ObjectDisposedException)
            {

            }
        }
        #endregion

        #region Sync
        private void StartSync()
        {
            
        }
        #endregion
    }
}
