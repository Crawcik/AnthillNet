using System.Net.Sockets;

namespace AnthillNet.API
{
    public class Connection
    {
        public Socket socket { private set; get; }
        public Connection(Socket socket) => this.socket = socket;
    }
}
