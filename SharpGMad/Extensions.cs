using System.Collections.Generic;
using System.Text;

namespace System
{
    /// <summary>
    /// Constains extensions for Int32 (int).
    /// </summary>
    static class Int32Extensions
    {
        /// <summary>
        /// The list of file size suffices to use when converting sizes to human-readable format.
        /// </summary>
        static string[] suffices = new string[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        /// <summary>
        /// Converts the specified file size (in bytes) to human-readable format.
        /// </summary>
        /// <param name="size">The file size in bytes.</param>
        /// <returns>Human-readable byte string</returns>
        public static string HumanReadableSize(this int size)
        {
            if (size == 0)
                return string.Format("{0}{1:0.#} {2}", null, 0, suffices[0]);

            double absSize = Math.Abs(size);
            double power = Math.Log(absSize, 1024);
            int unit = (int)power >= suffices.Length
                ? suffices.Length - 1
                : (int)power;
            double normSize = absSize / Math.Pow(1024, unit);

            return string.Format(
                "{0}{1:0.#} {2}",
                size < 0 ? "-" : null, normSize, suffices[unit]);
        }
    }
}

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