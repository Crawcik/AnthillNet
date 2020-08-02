
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnthillNet;
using System.Threading;

namespace UnitTest
{
    [TestClass]
    public class Runtime
    {
        [TestMethod]
        public static void Main()
        {
            Server server = new Server(ProtocolsType.TCP);
            server.Init(8);
            server.Start(7783);
            Thread.Sleep(1000);
            Client client = new Client(ProtocolsType.TCP);
            client.Init(8);
            client.Connect("192.168.1.101", 7783);
            Thread.Sleep(-1);
        }
    }
}
