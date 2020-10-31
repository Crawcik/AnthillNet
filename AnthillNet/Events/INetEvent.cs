namespace AnthillNet.Events
{
    public interface INetEvent
    {
    }

    [System.Serializable]
    public abstract class NetArgs
    {
        public abstract void Invoke(INetEvent ev);
    }
}
