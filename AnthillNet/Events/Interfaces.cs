using AnthillNet.Events.Entities;

namespace AnthillNet.Events
{
    public interface ILatency_NetEvent : INetEvent
    {
        void OnLatencyResult(Latency_NetArgs args);
    }
}
