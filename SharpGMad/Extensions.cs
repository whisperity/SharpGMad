using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// Contains extensions for BinaryWriter.
    /// </summary>
    static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a custom string to the BinaryWriter terminated with a NULL (0x00) character.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public static void WriteNullTerminatedString(this BinaryWriter bw, string str)
        {
            bw.Write(Encoding.ASCII.GetBytes(str));
            bw.Write((byte)0x00);
        }
    }

    /// <summary>
    /// Contains extensions for BinaryReader.
    /// </summary>
    static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a NULL (0x00) terminated string from the BinaryReader starting from the current position.
        /// </summary>
        /// <returns>The string read, converted from bytes using ASCII encoding.</returns>
        public static string ReadNullTerminatedString(this BinaryReader br)
        {
            List<byte> bytes = new List<byte>();
            byte read;

            while ((read = br.ReadByte()) != 0x00)
                bytes.Add(read);

            return (bytes.Count > 0 ? Encoding.ASCII.GetString(bytes.ToArray()) : "");
        }
    }
}