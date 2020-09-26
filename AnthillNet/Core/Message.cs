using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace AnthillNet.Core
{
    [Serializable]
    public struct Message
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();
        public ulong destiny { private set; get; }
        public object data { private set; get; }

        public Message(ulong destiny, object data)
        {
            this.destiny = destiny;
            this.data = data;
        }

        public static Message Deserialize(byte[] raw_data)
        {
            Message message;
            try
            {
                message = (Message)formatter.Deserialize(new MemoryStream(raw_data));
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            return message;
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, this);
            }
            catch (SerializationException e)
            {
                throw e;
            }
            finally
            {
                stream.Close();
            }
            return stream.ToArray();
        }
    }
}
