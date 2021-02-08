using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnthillNet;
using AnthillNet.Events;
using System;
using System.IO;
using System.Threading;

namespace TestingLab
{
    [TestClass]
    public class CoreTest
    {
        [TestMethod]
        public void TCP()
        {

            HostSettings settings = new HostSettings()
            {
                LogPriority = AnthillNet.Core.LogType.Debug,
                Protocol = AnthillNet.Core.ProtocolType.TCP,
                Port = 7777,
                TickRate = 8,
                Async = true,
                DualChannels = true,
                WriteLogsToConsole = true,
                MaxConnections = 0,
                MaxDataSize = 2048
            };

            Host server = new Host(HostType.Server);
            Host client = new Host(HostType.Client);
            server.Settings = settings;
            client.Settings = settings;
            server.Start();
            Thread.Sleep(2000);
            client.Start();
            Thread.Sleep(2000);
            client.Order.Call(Test, "Ja jestem client");
            Thread.Sleep(1000);
            server.Order.Call(Test, "Ja jestem serwer");
            Thread.Sleep(2000);
            server.Stop();
            Thread.Sleep(2000);
            client.Stop();
            Thread.Sleep(2000);
            server.Dispose();
            client.Dispose();

        }

        [Order]
        void Test(object tekst)
        {
            Console.WriteLine((string)tekst);
        }

        void Update(ref Host host)
        {
            while (host.Transport.Active)
            {
                if (host.Connections != null)
                    host.Transport.Logging.Log(host.Connections.Count.ToString(), AnthillNet.Core.LogType.Debug);
                Thread.Sleep(500);
            }
        }
    }
}
