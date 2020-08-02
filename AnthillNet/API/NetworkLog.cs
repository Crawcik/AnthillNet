using System;

namespace AnthillNet.API
{
    public delegate void NetworkLogHandler(object sender, NetworkLogArgs e);
    public class NetworkLog
    {
        public string LogName;

        public LogType LogPriority = LogType.Error;

        public event NetworkLogHandler OnNetworkLog;

        public void Log(string message, LogType priority = LogType.Info)
        {
            if ((int)priority <= (int)LogPriority)
                OnNetworkLog?.Invoke(this, new NetworkLogArgs(LogName, message, priority));
        }
    }

    public struct NetworkLogArgs
    {
        public DateTime Time { private set; get; }
        public string LogName { private set; get; }
        public LogType Priority { private set; get; }
        public string Message { private set; get; }
        public NetworkLogArgs(string sender, string message, LogType priority = LogType.Info)
        {
            this.Time = DateTime.Now;
            this.LogName = sender;
            this.Priority = priority;
            this.Message = message;
        }
    }
}
