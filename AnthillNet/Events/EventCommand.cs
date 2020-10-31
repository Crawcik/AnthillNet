using System;

namespace AnthillNet.Events
{
    [Serializable]
    public class EventCommand
    {
        public Type type;
        public NetArgs args;
    }
}
