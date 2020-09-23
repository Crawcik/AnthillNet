using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using AnthillNet.Core;

namespace UnitTest
{
    [TestClass]
    public class Runtime
    {
        public static int message_count;

        [TestMethod]
        public static void Main()
        {
            Test();
            Thread.Sleep(-1);
        }

        public static void Test()
        {
            Server server = new Server(ProtocolType.TCP);
            server.Logging.LogPriority = LogType.Debug;
            server.Logging.OnNetworkLog += OnNetworkLog;
            server.Init();
            server.Start(7783);
            Client client = new Client(ProtocolType.TCP);
            client.Logging.LogPriority = LogType.Debug;
            client.Logging.OnNetworkLog += OnNetworkLog;
            client.Init();
            client.Connect("127.0.0.1", 7783);
            client.Send(0, Console.ReadLine());
        }

        private static void OnRevieceMessage(object sender, Message[] messages)
        {
            string message = string.Join("\n", messages.Select(x => (string)x.data));
            ((Host)sender).Logging.Log("Messages:\n" + message);
        }

        private static void OnNetworkLog(object sender, NetworkLogArgs e)
        {
            Console.Write($"[{e.Time}]");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write($"[{e.LogName}]");
            switch (e.Priority)
            {
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
            }
            Console.Write($"[{e.Priority}]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{e.Message}\n");
        }
    }
}
