using System;

namespace AnthillNet.Events.Entities
{
    [System.Serializable]
    public class PingResult_NetArgs : NetArgs
    {
        public PingResult_NetArgs(double time) => this.time = time;
        public readonly double time;

        public override void Invoke(INetEvent ev) => ((IPingResult_NetEvent)ev).OnPingResult(this);
    }
}
