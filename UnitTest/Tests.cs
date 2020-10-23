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
        Server server = new Server();
        server.Logging.LogPriority = LogType.Debug;
        server.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
        server.OnReceiveMessages += Fuctions.OnRevieceMessage;
        server.Init(Protocol, TickRate);
        server.Start("127.0.0.1", Port);
        Console.WriteLine("Server IP:");
        string IP = Console.ReadLine();
        Client client = new Client();
        client.Logging.LogPriority = LogType.Debug;
        client.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
        client.OnReceiveMessages += Fuctions.OnRevieceMessage;
        client.Init(Protocol,TickRate);
        client.Start(IP, Port);
        loop:
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.S:
                Console.Write("> ");
                server.SendToAll(new Message(0, Console.ReadLine()));
                break;
            case ConsoleKey.C:
                Console.WriteLine("> ");
                client.Send(new Message(0, Console.ReadLine()), null);
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
        Console.WriteLine("Port:");
        ushort Port = ushort.Parse(Console.ReadLine());
        Console.WriteLine("TickRate:");
        byte TickRate = byte.Parse(Console.ReadLine());
        Console.WriteLine("Protocol:");
        ProtocolType Protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), Console.ReadLine());
        Console.WriteLine("\nServer IP:");
        string IP = Console.ReadLine();
        Console.WriteLine("Press to be:\n\tS - Server\n\tAny - Client");
        string text;
        if (Console.ReadKey().Key == ConsoleKey.S)
        {
            Console.WriteLine();
            Server server = new Server();
            server.Logging.LogPriority = LogType.Debug;
            server.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
            server.OnReceiveMessages += Fuctions.OnRevieceMessage;
            server.Init(Protocol, TickRate);
            server.Start(IP, Port) ;
            while ((text = Console.ReadLine()) != "exit")
                server.SendToAll(new Message(0, text));
            server.Dispose();
        }
        else
        {
            Client client = new Client();
            client.Logging.LogPriority = LogType.Debug;
            client.Logging.OnNetworkLog += Fuctions.OnNetworkLog;
            client.OnReceiveMessages += Fuctions.OnRevieceMessage;
            client.Init(Protocol, TickRate);
            client.Start(IP, Port);
            while ((text = Console.ReadLine()) != "exit")
                client.Send(new Message(0, text), null);
            client.Dispose();
        }
    }
}
