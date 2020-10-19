using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    internal class ServerUDP : Base
    {
        private UdpClient Listener;
        private Dictionary<EndPoint, Connection> Dictionary;
        private IPEndPoint PublicEP;

        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.Listener = new UdpClient(port);
            this.Dictionary = new Dictionary<EndPoint, Connection>();
            this.PublicEP = new IPEndPoint(IPAddress.Any, port);
            this.Listener.BeginReceive(WaitForMessage, null);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
        }
        private void OnStopped()
        {
            this.Listener.Client.Dispose();
            this.Dictionary.Clear();
            base.OnStop -= OnStopped;
        }

        public override void Stop()
        {
            if (!this.Active) return;
            this.Listener.Close();
            base.Stop();
        }

        protected override void Tick()
        {
            foreach (Connection connection in this.Dictionary.Values)
                if (connection.MessagesCount > 0)
                {
                    base.IncomingMessagesInvoke(connection);
                    connection.ClearMessages();
                }
            base.Tick();
        }

        private void WaitForMessage(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = this.Listener.EndReceive(ar, ref PublicEP);
                if (!this.Dictionary.ContainsKey(PublicEP))
                {
                    Connection connection = new Connection(PublicEP);
                    this.Dictionary.Add(PublicEP, connection);
                    base.Connect(connection);
                }
                this.Dictionary[PublicEP].Add(Message.Deserialize(buffer));
                this.Listener.BeginReceive(WaitForMessage, null);
            }
            catch (SocketException e)
            {
                base.InternalHostErrorInvoke(e);
            }
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            this.Listener.BeginSend(buf, buf.Length, IPAddress, (IAsyncResult ar) => this.Listener.EndSend(ar), null);
        }
    }

    internal class ClientUDP : Base
    {
        private UdpClient Client;
        private Connection connection;
        private IPEndPoint ServerEP;

        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.Client = new UdpClient();
            this.ServerEP = new IPEndPoint(IPAddress.Parse(hostname), port);
            this.connection = new Connection(this.ServerEP);
            this.Client.Connect(this.ServerEP);
            this.Client.BeginReceive(WaitForMessage, null);
            base.OnStop += OnStopped;
            base.Start(hostname, port);
            base.Connect(connection);
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
        public override void Disconnect(Connection connection)
        {
            base.Disconnect(connection);
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
            this.Client.Close();
            base.OnStop -= OnStopped;
        }
        private void WaitForMessage(IAsyncResult ar)
        {
            try
            {

                byte[] buffer = this.Client.EndReceive(ar, ref this.ServerEP);
                this.connection.Add(Message.Deserialize(buffer));
                this.Client.BeginReceive(WaitForMessage, null);
                base.Connect(this.connection);
            }
            catch (Exception e)
            {
                this.Client.Close();
                //base.InternalHostErrorInvoke(e);
                base.Disconnect(connection);
            }
        }
        public override void Send(Message message, IPEndPoint IPAddress)
        {
            byte[] buf = message.Serialize();
            this.Client.Client.BeginSend(buf, 0, buf.Length, 0, (IAsyncResult ar) => this.Client.Client.EndSend(ar), null);
        }
    }
}
