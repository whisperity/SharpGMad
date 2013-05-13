using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    static class BinaryWriterExtensions
    {
        public static void WriteString(this BinaryWriter bw, string str)
        {
            bw.Write(Encoding.ASCII.GetBytes(str));
            bw.Write((byte)0x00);
        }
    }

    static class BinaryReaderExtensions
    {
        public static string ReadString(this BinaryReader br)
        {
            List<byte> bytes = new List<byte>();
            byte read;

            while ((read = br.ReadByte()) != 0x00)
                bytes.Add(read);

            return (bytes.Count > 0 ? Encoding.ASCII.GetString(bytes.ToArray()) : "");
        }
    }
}