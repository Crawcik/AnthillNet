﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Server : Host
    {
        private Dictionary<EndPoint, Connection> Dictionary;

        public override void Start(string hostname, ushort port)
        {
            this.Dictionary = new Dictionary<EndPoint, Connection>();
            HostSocket.Bind(new IPEndPoint(IPAddress.Parse(hostname), port));
            HostSocket.Listen(100);
            Logging.Log($"Starting listening on port {port} with {Enum.GetName(typeof(ProtocolType), Protocol)} protocol", LogType.Debug);
            this.HostSocket.BeginAccept(WaitForConnection, null);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
        }

        private void OnStopped()
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

        protected override void Tick()
        {
            try
            {
                foreach (Connection connection in this.Dictionary.Values)
                    if (connection.MessagesCount > 0)
                    {
                        this.Logging.Log($"Message from {connection.EndPoint}: Count {connection.MessagesCount}", LogType.Debug);
                        base.IncomingMessagesInvoke(connection);
                        connection.ClearMessages();
                    }
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
            base.Tick();
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                Socket client = this.HostSocket.EndAccept(ar);
                Connection connection = new Connection(client);
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                this.Logging.Log($"Client {client.RemoteEndPoint} connected", LogType.Info);
                base.Connect(connection);
                client.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, client);
                this.HostSocket.BeginAccept(WaitForConnection, null);
            }
            catch (Exception e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndReceive(ar);
                byte[] buffer = new byte[this.MaxMessageSize];
                socket.Receive(buffer, buffer.Length, 0);
                this.Dictionary[socket.RemoteEndPoint].Add(Message.Deserialize(buffer));
                socket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, socket);
            }
            catch (Exception e)
            {
                this.Logging.Log($"Client {socket.RemoteEndPoint} disconnect from server", LogType.Info);
                this.Dictionary.Remove(socket.RemoteEndPoint);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                base.InternalHostErrorInvoke(e);
            }
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            Socket socket = this.Dictionary[IPAddress].Socket;
            socket.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => socket.EndSend(ar), null);
        }

        public void SendToAll(Message message, IPEndPoint address)
        {
            foreach (Connection ip in Dictionary.Values)
                Send(message, ip.EndPoint);
        }

        public override void Dispose()
        {
            this.Logging.Log($"Disposing", LogType.Debug);
            this.ForceStop();
            base.Dispose();
        }
    }
}
