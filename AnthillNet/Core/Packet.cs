namespace AnthillNet.Core
{
    public readonly struct Packet
    {
        public readonly Connection Connection { get; }
        public readonly byte[] Data { get; }

        internal Packet(Connection connection, byte[] data)
        {
            this.Connection = connection;
            this.Data = data;
        }
    }
}
