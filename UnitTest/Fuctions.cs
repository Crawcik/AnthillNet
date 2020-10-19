using System;
using System.Linq;
using AnthillNet.Core;

public static class Fuctions
{
    public static void OnRevieceMessage(object sender, Connection connection)
    {
        string message = string.Join("\n", connection.GetMessages().Select(x => (string)x.data));
        ((Host)sender).Logging.Log("Messages:\n" + message);
    }

    public static void OnNetworkLog(object sender, NetworkLogArgs e)
    {
        Console.Write($"[{e.Time.ToString("HH:mm:ss")}]");
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
