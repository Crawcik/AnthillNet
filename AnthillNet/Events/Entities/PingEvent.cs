namespace AnthillNet.Events.Entities
{
    public class PingResult_NetArgs : NetArgs
    {
        public override void Invoke(INetEvent ev) => ((IPingResult_NetEvent)ev).OnPingResult(this);
    }
}
