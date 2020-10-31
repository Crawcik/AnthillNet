using AnthillNet.Events.Entities;

namespace AnthillNet.Events
{
    public interface IPingResult_NetEvent : INetEvent
    {
        void OnPingResult(PingResult_NetArgs args);
    }

    public interface ITest_NetEvent : INetEvent
    {
        void OnTest(PingResult_NetArgs args);
    }
}
