# *AnthillNet* is a library for network solutions with simple features and a lot of customization settings
## Currently project is completed and I don't think it will be continued due to the lack of people using this repository

### What it can I do in it?
* **Send** and **receive** simple packet or serialized messages
* You can make **events**, that can be invoke by server or client
* Calling server or other clients to **execute method**
* Get **logs** of operations preformed by host

### What settings can I change in it?
* Change protocol to **UDP** or **TCP**
* Start, Stop, Pause and Resume **host**
* Change host **tickrate** from 0 to 255
* Change to sync or async functions
* Control ticks on your own without making new threads
* Change log priority

#### You can also use:
1. Only _AnthillNet.Core_ namespace without _AnthillNet.Event_ and _AnthillNet_ namespace
1. _AnthillNet.Core_ and _AnthillNet.Event_ namespace without _AnthillNet_

#### Example code for server or client:
```cs
using AnthillNet;
using AnthillNet.Core;
using AnthillNet.Events;

using System;

public class Program
{
    public const ushort myPort = 7777;

    public static void Main() => new Program().Run();

    public void Run()
    {
        HostSettings settings = new HostSettings()
        {
            LogPriority = LogType.Info,
            Port = 8000,
            Protocol = ProtocolType.TCP,
            TickRate = 8,
            Async = true,
            WriteLogsToConsole = true,
            MaxConnections = 20,
            MaxDataSize = 4096
        };

        Host host = new Host(HostType.Server); //Or HostType.Client for client obviously
        host.Settings = settings;

        host.Start(); //FOR SERVER: You can also set IP but then you can only connect through this one
        host.Send(new Message(0, "Test")); //Sending simple message (destinies from 1 to 100 are reserved for events, orders, pings etc. if you using AnthillNet.Events)
        while (true)
        {
            char x = Console.ReadKey().KeyChar;
            switch (x)
            {
                case 'o':
                    host.Order.Call(OrderTest); //You can also send method with argument
                    break;
            }
            if (x == 'q')
                break;
        }
        host.Stop();
        host.Dispose();

    }

    [Order(toClient: true, toServer: true)]
    public void OrderTest()
    {
        Console.WriteLine("This method is called by client or server");
    }
}
```
