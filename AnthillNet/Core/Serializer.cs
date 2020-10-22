using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace AnthillNet.Core
{
    public delegate byte[] Serializer(Message value);
    public delegate Message Deserializer(byte[] data);
    public class Serialization
    {
        public static Serializer Serializer { internal get; set; }
        public static Deserializer Deserializer { internal get; set; }

        static Serialization() => SetDefault();
        public static void SetDefault()
        {
            Serializer = new Serializer(DefaultSerializer);
            Deserializer = new Deserializer(DefaultDeserializer);
        }

        private static Message DefaultDeserializer(byte[] data)
        {
            Message message;
            try
            {
                message = (Message)new BinaryFormatter().Deserialize(new MemoryStream(data));
            }
            catch (SerializationException e)
            {
                throw e;
            }
            return message;
        }

        private static byte[] DefaultSerializer(Message value)
        {
            MemoryStream stream = new MemoryStream();
            try
            {
                new BinaryFormatter().Serialize(stream, value);
            }
            finally
            {
                stream.Close();
            }
            return stream.ToArray();
        }
    }
}
