namespace AnthillNet.Core
{
    public struct Packet
    {
        public IConnection Connection { get; }
        public byte[] Data { get; }

        internal Packet(Connection connection, byte[] data)
        {
            this.Connection = connection;
            this.Data = data;
        }
    }
}
