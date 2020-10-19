using System;
using System.Net.Sockets;

namespace AnthillNet.Core
{
    public sealed class Client : Host
    {

        public Connection Host { get; private set; }

        public override void Start(string hostname, ushort port)
        {
            if (this.Active) return;
            this.Client = new Socket(SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp)
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
