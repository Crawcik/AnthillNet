using AnthillNet.Core;

namespace AnthillNet
{
    [System.Serializable]
    public struct HostSettings
    {
        //Usefull for have logs for multiple server
        public string Name;

        //How many times in second host will check & read data from connections
        public byte TickRate;

        //How much clients can connect to server (Set to 0 if you only client)
        public uint MaxConnections;

        //Is this working async
        public bool Async;

        //Is it be reading both IPv4 and IPv6
        public bool DualChannels;

        //How big can single packet can be (in bytes)
        public int MaxDataSize;

        //Logging to console
        public bool WriteLogsToConsole;

        //Network Protocol
        public ProtocolType Protocol;

        //Logs Priority
        public LogType LogPriority;
    }
}
