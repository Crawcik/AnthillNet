namespace AnthillNet.Core
{
    public struct Packet
    {
        public readonly Connection connection;
        public readonly byte[] data;

        internal Packet(Connection connection, byte[] data)
        {
            this.connection = connection;
            this.data = data;
        }
    }
}
