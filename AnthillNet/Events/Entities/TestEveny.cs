namespace AnthillNet.Events.Entities
{
    [System.Serializable]
    public class Test_NetArgs : NetArgs
    {
        public override void Invoke(INetEvent ev) => ((ITest_NetEvent)ev).OnTest(this);
    }
}
