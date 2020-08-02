using System.Net;
using System.Net.Sockets;

namespace AnthillNet.API
{
    internal class ServerTCP : Base
    {
        private TcpListener Listener;
        public override void Start(string hostname, ushort port)
        {
            if (Active) return;
            Listener = TcpListener.Create(port);
            Listener.Start();
            base.Start(hostname, port);
        }
        public override void Stop()
        {
            if (!Active) return;
            Listener.Stop();
            base.Stop();
        }
        public override void ForceStop()
        {
            if (!Active) return;
            Listener.Stop();
            base.ForceStop();
        }

        protected override void Tick()
        {
            if (Listener.Pending())
            {
                ConnectionStabilizedInvoke(new Connection(Listener.AcceptSocket()));
            }
            base.Tick();
        }
    }

    internal class ClientTCP : Base
    {
        TcpClient Client;
        bool isConnecting = false;

        public override void Start(string hostname, ushort port)
        {
            if (Active) return;
            Client = new TcpClient();
            IPAddress iPAddress = IPAddress.Parse(hostname);
            Client.Connect(iPAddress, port);
            isConnecting = true;
            base.Start(hostname, port);
        }
        public override void Stop()
        {
            if (!Active) return;
            Client.Close();
            base.Stop();
        }
        public override void ForceStop()
        {
            if (!Active) return;
            Client.Close();
            base.ForceStop();
        }

        protected override void Tick()
        {
            if(isConnecting)
                if(Client.Connected)
                {
                    ConnectionStabilizedInvoke(new Connection(Client.Client));
                    isConnecting = false;
                }
            base.Tick();
        }
    }
}
