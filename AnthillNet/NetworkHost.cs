using AnthillNet.Core;
using System;
using System.Net;

namespace AnthillNet
{
    public class Host
    {
        public Base Transport { private set; get; }
        public HostType Type { private set; get; }
        public HostSettings Settings { private set; get; }

        private Host() { }
        public Host(HostType type) {
            this.Transport = (this.Type = type) switch
            {
                HostType.Server => new Server(),
                HostType.Client => new Client(),
                _ => throw new System.NotImplementedException()

            };
            this.Settings = new HostSettings()
            {
                MaxConnections = 0,
                MaxDataSize = 4096,
                TickRate = 8,
                WriteLogsToConsole = false,
                Protocol = ProtocolType.TCP,
                LogPriority = LogType.Error
            };
        }

        public void Start(string hostname, ushort port)
        {
            this.Transport.Logging.LogPriority = this.Settings.LogPriority;
            this.Transport.Logging.OnNetworkLog += OnNetworkLog;
            this.Transport.OnReceiveMessages += OnRevieceMessage;
            this.Transport.Init(this.Settings.Protocol, this.Settings.TickRate);
            IPAddress ip;
            if (!this.ResolveIP(hostname, out ip))
                return;
            this.Transport.Start(ip, port);
        }

        public void Start(ushort port) => this.Start("127.0.0.1", port);

        private void OnNetworkLog(object sender, NetworkLogArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnRevieceMessage(object sender, Connection connection)
        {
            throw new NotImplementedException();
        }

        private bool ResolveIP(string hostname, out IPAddress iPAddress)
        {
            switch (Uri.CheckHostName(hostname))
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    if (IPAddress.TryParse(hostname, out iPAddress))
                        return true;
                    break;
                case UriHostNameType.Dns:
                    IPHostEntry hostEntry;
                    if ((hostEntry = Dns.GetHostEntry(hostname)).AddressList.Length > 0)
                    {
                        iPAddress = hostEntry.AddressList[0];
                        return true;
                    }
                    break;
            }
            this.Transport.Logging.Log("Given hostname is invalid!", LogType.Error);
            iPAddress = null;
            return false;
        }
    }
}
