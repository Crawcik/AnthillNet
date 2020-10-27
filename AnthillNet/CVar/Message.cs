namespace AnthillNet.CVar
{
    [System.Serializable]
    public struct Message
    {
        public readonly string destiny;
        public readonly object data;

        public Message(string destiny, object data)
        {
            this.destiny = destiny;
            this.data = data;
        }

        public byte[] Serialize() => Serialization.Serializer.Invoke(this);
        public static Message Deserialize(byte[] raw_data) => Serialization.Deserializer.Invoke(raw_data);
    }
}
