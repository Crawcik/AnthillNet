namespace AnthillNet.Events
{
    [System.Serializable]
    public struct Message
    {
        public readonly ulong destiny;
        public readonly object data;

        public Message(ulong destiny, object data)
        {
            this.destiny = destiny;
            this.data = data;
        }

        public byte[] Serialize() => Serialization.Serializer.Invoke(this);
        public static Message Deserialize(byte[] raw_data) => Serialization.Deserializer.Invoke(raw_data);
    }
}
