namespace AnthillNet.Events
{
    public interface INetEvent
    {
        void Invoke(INetEvent ev);
    }

    [System.Serializable]
    public abstract class NetArgs : INetEvent
    {
        public abstract void Invoke(INetEvent ev);
    }
}
