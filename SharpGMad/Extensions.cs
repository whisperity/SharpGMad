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
            bw.Write(Encoding.UTF8.GetBytes(str));
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

            return (bytes.Count > 0 ? Encoding.UTF8.GetString(bytes.ToArray()) : "");
        }
    }

    /// <summary>
    /// Provides extra methods towards file on the local filesystem.
    /// </summary>
    static class FileExtensions
    {
        /// <summary>
        /// Checks if you can write to a specific file.
        /// </summary>
        /// <param name="filename">The path to the file on the local filesystem.</param>
        /// <returns>A boolean whether the file is writable.</returns>
        public static bool CanWrite(string filename)
        {
            //Check if the file exists
            if (!File.Exists(filename))
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");

            try
            {
                // Open a new FileStream and test if it's writable
                new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Dispose();
                return true; // It is
            }
            catch (Exception)
            {
                // In case any error happens, the file is not writable.
                return false;
            }
        }
    }
}