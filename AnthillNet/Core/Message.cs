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
        public ulong destiny { private set; get; }
        public object data { private set; get; }

        public Message(ulong destiny, object data)
        {
            this.destiny = destiny;
            this.data = data;
        }

        public byte[] Serialize() => Serialization.Serializer.Invoke(this);
        public static Message Deserialize(byte[] raw_data) => Serialization.Deserializer.Invoke(raw_data);
    }
}
