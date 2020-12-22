using System.Diagnostics.Contracts;
using BinaryPack.Serialization.Processors;

using BinaryReader = BinaryPack.Serialization.Buffers.BinaryReader;
using BinaryWriter = BinaryPack.Serialization.Buffers.BinaryWriter;

namespace BinaryPack
{
    /// <summary>
    /// The entry point <see langword="class"/> for all the APIs in the library
    /// </summary>
    public static class BinaryConverter
    {

        public static byte[] Serialize<T>(T obj) where T : new()
        {
            BinaryWriter writer = new BinaryWriter();

            try
            {
                ObjectProcessor<T>.Instance.Serializer(obj, ref writer);

                return writer.Span.ToArray();
            }
            finally
            {
                writer.Dispose();
            }
        }

        [Pure]
        unsafe public static T Deserialize<T>(ref byte[] data) where T : new()
        {
            byte* pointer = stackalloc byte[data.Length];
            BinaryReader reader;
            lock (data)
            {
                reader = new BinaryReader(pointer);
                return ObjectProcessor<T>.Instance.Deserializer(ref reader);
            }
        }
    }
}
