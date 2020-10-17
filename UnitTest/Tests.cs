using System;
using AnthillNet.Core;
public static class Tests
{
    /// <summary> Keys info:
    /// Q - Quit
    /// C - Send string to server as client
    /// S - Send string to all clients as server
    /// </summary>
    public static void Test1()
    {
        Console.WriteLine("Port:");
        ushort Port = ushort.Parse(Console.ReadLine());
        Console.WriteLine("TickRate:");
        byte TickRate = byte.Parse(Console.ReadLine());
        Console.WriteLine("Protocol:");
        ProtocolType Protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), Console.ReadLine());
        Server server = new Server(Protocol);
        server.Logging.LogPriority = LogType.Debug;
        server.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
        server.OnRevieceMessage += Fuctions.OnRevieceMessage;
        server.Init(TickRate);
        server.Start(Port);
        Console.WriteLine("Server IP:");
        string IP = Console.ReadLine();
        Client client = new Client(ProtocolType.TCP);
        client.Logging.LogPriority = LogType.Debug;
        client.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
        client.OnRevieceMessage += Fuctions.OnRevieceMessage;
        client.Init(4);
        client.Connect(IP, 7783);
        loop:
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.S:
                Console.Write("> ");
                server.Send(0, Console.ReadLine());
                break;
            case ConsoleKey.C:
                Console.WriteLine("> ");
                client.Send(0, Console.ReadLine());
                break;
            case ConsoleKey.Q:
                server.Dispose();
                client.Dispose();
                return;
        }
        goto loop;
    }

    public static void Test2()
    {
        ushort Port = 7777;
        byte TickRate = 32;
        ProtocolType Protocol = ProtocolType.UDP;
        Console.WriteLine("Press to be:\n\tS - Server\n\tAny - Client");
        string text;
        if (Console.ReadKey().Key == ConsoleKey.S)
        {
            Console.WriteLine();
            Server server = new Server(Protocol);
            server.Logging.LogPriority = LogType.Debug;
            server.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
            server.OnRevieceMessage += Fuctions.OnRevieceMessage;
            server.Init(TickRate);
            server.Start(Port);
            while ((text = Console.ReadLine()) != "exit")
                server.Send(1, text);
            server.Dispose();
        }
        else
        {
            Console.WriteLine("\nServer IP:");
            string IP = Console.ReadLine();
            Console.WriteLine();
            Client client = new Client(Protocol);
            client.Logging.LogPriority = LogType.Debug;
            client.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
            client.OnRevieceMessage += Fuctions.OnRevieceMessage;
            client.Init(TickRate);
            client.Connect(IP, Port);
            while ((text = Console.ReadLine()) != "exit")
                client.Send(1, text);
            client.Dispose();
        }
    }
}
