using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    internal class ServerTCP : Base
    {
        private TcpListener Listener;
        private Dictionary<EndPoint, Connection> Dictionary;
        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.Listener = TcpListener.Create(port);
            this.Dictionary = new Dictionary<EndPoint, Connection>();
            this.Listener.Start();
            this.Listener.BeginAcceptSocket(WaitForConnection, null);
            base.Start(hostname, port);
        }
        public override void Stop()
        {
            if (!this.Active) return;
            this.Listener.Stop();
            base.Stop();
        }
        public override void ForceStop()
        {
            if (!this.Active) return;
            this.Listener.Stop();
            base.ForceStop();
        }

        protected override void Tick()
        {
            try
            {
                foreach (Connection connection in this.Dictionary.Values)
                    if (connection.MessagesCount > 0)
                        this.IncomingMessagesInvoke(connection);
            }
            catch (Exception e)
            {
                this.InternalHostErrorInvoke(e);
            }
            base.Tick();
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                Socket client = this.Listener.EndAcceptSocket(ar);
                Connection connection = new Connection(client.RemoteEndPoint);
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                this.Connect(connection);
                client.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, client);
                this.Listener.BeginAcceptSocket(WaitForConnection, null);
            } 
            catch (Exception e)
            {
                this.InternalHostErrorInvoke(e);
            }
            finally
            {
                this.Listener.Server.Dispose();
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
                this.Dictionary[socket.RemoteEndPoint].AddReceived(Message.Deserialize(buffer));
                socket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, socket);
            }
            catch (Exception e)
            {
                this.InternalHostErrorInvoke(e);
            }
            finally
            {
                socket.Dispose();
                this.Dictionary.Clear();
            }
        }
    }

    internal class ClientTCP : Base
    {
        private TcpClient Client;
        private Connection connection;
        private bool isConnecting;

        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.Client = new TcpClient();
            IPAddress iPAddress = IPAddress.Parse(hostname);
            this.isConnecting = true;
            this.Client.Connect(iPAddress, port);
            base.Start(hostname, port);
        }
        public override void Stop()
        {
            if (!this.Active) return;
            this.Client.Close();
            base.Stop();
        }
        public override void ForceStop()
        {
            if (!this.Active) return;
            this.Client.Close();
            base.ForceStop();
        }

        protected override void Tick()
        {
            if (this.isConnecting)
                if (this.Client.Connected)
                {
                    this.connection = new Connection(Client.Client.RemoteEndPoint);
                    base.Connect(connection);
                    this.Client.Client.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, Client.Client);
                    this.isConnecting = false;
                }
            if (this.connection.EndPoint != null)
                if (this.connection.MessagesCount > 0)
                    this.IncomingMessagesInvoke(this.connection);
            base.Tick();
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndReceive(ar);
                byte[] buffer = new byte[this.MaxMessageSize];
                socket.Receive(buffer, buffer.Length, 0);
                this.connection.AddReceived(Message.Deserialize(buffer));
                socket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, socket);
            }
            catch (Exception e)
            {
                InternalHostErrorInvoke(e);
            }
        }
        public override void Send(Message message, string IPAddress)
        {
            this.Client.Client.Send(message.Serialize());
        }
    }
}
