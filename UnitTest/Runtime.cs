using System;
using System.Linq;
using AnthillNet.Core;

Test1();

void Test1()
{
    Console.WriteLine("Port:");
    ushort Port = ushort.Parse(Console.ReadLine());
    Console.WriteLine("TickRate:");
    byte TickRate = byte.Parse(Console.ReadLine());
    Console.WriteLine("Protocol:");
    ProtocolType Protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), Console.ReadLine());
    Server server = new Server(Protocol);
    server.Logging.LogPriority = LogType.Debug;
    server.Logging.OnNetworkLog += OnNetworkLog;
    server.OnRevieceMessage += OnRevieceMessage;
    server.Init(TickRate);
    server.Start(Port);
    Console.WriteLine("Server IP:");
    string IP= Console.ReadLine();
    Client client = new Client(ProtocolType.TCP);
    client.Logging.LogPriority = LogType.Debug;
    client.Logging.OnNetworkLog += OnNetworkLog;
    client.OnRevieceMessage += OnRevieceMessage;
    client.Init(4);
    client.Connect(IP, 7783);
loop:
    if (Console.ReadKey().Key == ConsoleKey.S)
    {
        Console.WriteLine("> ");
        server.Send(0, Console.ReadLine());
    }
    else if (Console.ReadKey().Key == ConsoleKey.C)
    {
        Console.WriteLine("> ");
        client.Send(0, Console.ReadLine());
    }
    else if (Console.ReadKey().Key == ConsoleKey.Q)
        return;
goto loop;
}

void OnRevieceMessage(object sender, Message[] messages)
{
    string message = string.Join("\n", messages.Select(x => (string)x.data));
    ((Host)sender).Logging.Log("Messages:\n" + message);
}

void OnNetworkLog(object sender, NetworkLogArgs e)
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
