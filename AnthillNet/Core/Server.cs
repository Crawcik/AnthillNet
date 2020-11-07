﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Server : Base
    {
        public readonly Dictionary<string, Connection> Dictionary;
        private EndPoint LastEndPoint;

        public Server()
        {
            this.Logging.LogName = "Server";
            this.Dictionary = new Dictionary<string, Connection>();
        }

        #region Public methods
        public override void Start(IPAddress ip, ushort port, bool run_clock = true)
        {
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            this.HostSocket.Bind(endPoint);
            if (this.Protocol == ProtocolType.TCP)
            {
                this.HostSocket.Listen(100);
                if (this.Async)
                    this.HostSocket.BeginAccept(this.WaitForConnection, null);
                else
                    try
                    {
                        this.ClientConnected(this.HostSocket.Accept());
                    }
                    catch (Exception e)
                    {
                        if (e is SocketException)
                            base.InternalHostErrorInvoke(e);
                    }

            }
            else if(this.Protocol == ProtocolType.UDP)
            {
                this.LastEndPoint = new IPEndPoint(IPAddress.Any, port);
                Connection stack = new Connection(endPoint){ TempBuffer = new byte[this.MaxMessageSize] };
                this.HostSocket.BeginReceiveFrom(stack.TempBuffer, 0, this.MaxMessageSize, 0, ref this.LastEndPoint, WaitForMessageFrom, stack);
            }
            this.Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), this.Protocol)} protocol", LogType.Debug);
            base.Start(ip, port, run_clock);
        }
        public override void Stop()
        {
            if (!this.Active) return;
            this.Logging.Log($"Stopping...", LogType.Debug);
            if(Protocol == ProtocolType.TCP)
                foreach (Connection connection in this.Dictionary.Values)
                    connection.Socket.Close();
            this.Dictionary.Clear();
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
            this.Dictionary.Remove(connection.EndPoint.ToString());
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
            this.ForceStop();
            base.Dispose();
        }
        #endregion

        #region Private methods
        private void ClientConnected(Socket client)
        {
            Connection connection = new Connection(client);
            connection.TempBuffer = new byte[this.MaxMessageSize];
            this.Dictionary.Add(client.RemoteEndPoint.ToString(), connection);
            client.BeginReceive(connection.TempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, connection);
            this.Logging.Log($"Client {client.RemoteEndPoint} connected", LogType.Info);
            base.Connect(connection);
            if(this.Async)
                this.HostSocket.BeginAccept(this.WaitForConnection, null);
            else
                try {
                }
                catch (Exception e)
                {
                    if (e is SocketException)
                        base.InternalHostErrorInvoke(e);
                }
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                ClientConnected(this.HostSocket.EndAccept(ar));
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
                connection.Add(connection.TempBuffer);
                connection.Socket.BeginReceive(connection.TempBuffer, 0, this.MaxMessageSize, 0, this.WaitForMessage, connection);
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
                    this.Logging.Log($"Client {this.LastEndPoint} connected", LogType.Info);
                }
                this.Dictionary[this.LastEndPoint.ToString()].Add(connection.TempBuffer);
                this.HostSocket.BeginReceiveFrom(connection.TempBuffer, 0, this.MaxMessageSize, 0, ref this.LastEndPoint, this.WaitForMessageFrom, connection);
            }
            catch (SocketException)
            {
                this.HostSocket.Close();
            }
            catch(ObjectDisposedException)
            {
           
            }
        }
        #endregion
    }
}
