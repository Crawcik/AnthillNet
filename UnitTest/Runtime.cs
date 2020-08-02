using System;
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
            server.Logging.LogPriority = AnthillNet.API.LogType.Debug;
            server.Logging.OnNetworkLog += OnNetworkLog;
            server.Init(8);
            server.Start(7783);
            Thread.Sleep(1000);

            Client client = new Client(ProtocolsType.TCP);
            client.Logging.LogPriority = AnthillNet.API.LogType.Debug;
            client.Logging.OnNetworkLog += OnNetworkLog;
            client.Init(8);
            client.Connect("192.168.1.101", 7783);

            Thread.Sleep(-1);
        }


        private static void OnNetworkLog(object sender, AnthillNet.API.NetworkLogArgs e)
        {
            Console.WriteLine($"[{e.Time}][{e.LogName}][{e.Priority}] {e.Message}");
        }
    }
}
