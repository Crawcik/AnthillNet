using System;
using System.Collections.Generic;
using System.Net;

namespace AnthillNet.Core
{
    public sealed class Server : Host
    {
        List<Connection> Connections = new List<Connection>();

        public override void Start(string hostname, ushort port)
        {
            HostSocket.Bind(new IPEndPoint(IPAddress.Parse(hostname), port));
            HostSocket.Listen(100);
            base.Start(hostname, port);
        }

        public override void
    }
}
