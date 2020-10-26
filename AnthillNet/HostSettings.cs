using AnthillNet.Core;

namespace AnthillNet
{
    public struct HostSettings
    {
        //How many times in second host will check & read data from connections
        public byte TickRate;

        //How much clients can connect to server (Leave empty if you only client)
        public uint MaxConnections;

        //How big can single packet can be (in bytes)
        public uint MaxDataSize;

        //Logging to console
        public bool WriteLogsToConsole;

        //Network Protocol
        public ProtocolType Protocol;

        //Logs Priority
        public LogType LogPriority;
    }
}
