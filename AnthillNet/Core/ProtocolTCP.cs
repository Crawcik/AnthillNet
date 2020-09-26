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
            if (this.Listener.Pending())
            {
                Socket client = this.Listener.AcceptSocket();
                Connection connection = new Connection(client.RemoteEndPoint);
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                this.Connect(connection);
                client.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, client);
            }
            foreach (Connection connection in this.Dictionary.Values)
                if (connection.MessagesCount > 0)
                    this.IncomingMessagesInvoke(connection);
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
                this.Dictionary[socket.RemoteEndPoint].AddReceived(Message.Deserialize(buffer));
            }
            catch
            {

            }
            socket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, socket);
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
                //this.Dictionary[socket.RemoteEndPoint].AddReceived(Message.Deserialize(buffer));
            }
            catch
            {

            }
            socket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, socket);
        }
        public override void Send(Message message, string IPAddress)
        {
            this.Client.Client.Send(message.Serialize());
        }
    }
}
