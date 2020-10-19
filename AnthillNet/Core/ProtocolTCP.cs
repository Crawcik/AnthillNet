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
            base.OnStop += OnStopped;
            base.Start(hostname, port);
        }

        private void OnStopped()
        {
            this.Listener.Server.Dispose();
            this.Dictionary.Clear();
            base.OnStop -= OnStopped;
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
                    {
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
                Socket client = this.Listener.EndAcceptSocket(ar);
                Connection connection = new Connection(client);
                this.Dictionary.Add(client.RemoteEndPoint, connection);
                base.Connect(connection);
                client.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, client);
                this.Listener.BeginAcceptSocket(WaitForConnection, null);
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
    }

    internal class ClientTCP : Base
    {
        private TcpClient Client;
        private Connection connection;

        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.Client = new TcpClient();
            this.Client.BeginConnect(IPAddress.Parse(hostname), port, WaitForConnection, null);
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
            if (this.connection.EndPoint != null)
                if (this.connection.MessagesCount > 0)
                {
                    base.IncomingMessagesInvoke(this.connection);
                    this.connection.ClearMessages();
                }
            base.Tick();
        }
        private void OnStopped()
        {
            if (this.Client.Connected)
                this.Client.Close();
            base.OnStop -= OnStopped;
        }
        private void WaitForConnection(IAsyncResult ar)
        {
            try
            {
                this.Client.EndConnect(ar);
                Socket client = this.Client.Client;
                this.connection = new Connection(client);
                client.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, client);
                base.Connect(this.connection);
            }
            catch (Exception e)
            {
                //base.InternalHostErrorInvoke(e);
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
                this.connection.Add(Message.Deserialize(buffer));
                socket.BeginReceive(new byte[] { }, 0, 0, 0, WaitForMessage, socket);
            }
            catch (Exception e)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                //base.InternalHostErrorInvoke(e);
                base.Disconnect(connection);
            }
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            this.Client.Client.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => this.Client.Client.EndSend(ar, out SocketError er), null);
        }
    }
}