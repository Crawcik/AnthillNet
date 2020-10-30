using AnthillNet.Events.Entities;

namespace AnthillNet.Events
{
    interface IPingResult_NetEvent
    {
        void OnPingResult(PingResult_NetArgs args);
    }
}
