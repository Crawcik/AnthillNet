using AnthillNet.Events.Entities;

namespace AnthillNet.Events
{
    public interface ILatency_NetEvent : INetEvent
    {
        void OnLatencyResult(Latency_NetArgs args);
    }

    public interface ITest_NetEvent : INetEvent
    {
        void OnTest(Test_NetArgs args);
    }
}
