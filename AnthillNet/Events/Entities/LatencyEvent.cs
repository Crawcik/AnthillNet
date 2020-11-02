using System;

namespace AnthillNet.Events.Entities
{
    [System.Serializable]
    public class Latency_NetArgs : NetArgs
    {
        public Latency_NetArgs(double time) => this.time = time;
        public readonly double time;

        public override void Invoke(INetEvent ev) => ((ILatency_NetEvent)ev).OnLatencyResult(this);
    }
}