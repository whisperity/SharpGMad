using System.Collections.Generic;
using System.Text;
#if WINDOWS
using System;
using System.Drawing;
using System.Runtime.InteropServices;
#endif

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

    /// <summary>
    /// Contains extensions for Version.
    /// </summary>
    static class VersionExtensions
    {
        /// <summary>
        /// Gets the version of the executing assembly with omitting the trailing zeros.
        /// </summary>
        static public string Pretty()
        {
            System.Reflection.AssemblyInformationalVersionAttribute[] informalVersions =
                (System.Reflection.AssemblyInformationalVersionAttribute[])(System.Reflection.Assembly
                .GetEntryAssembly()
                .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false));

            System.Reflection.AssemblyInformationalVersionAttribute info = null;
            if (informalVersions.Length == 1) // There can be only one informal version for an assembly
                info = informalVersions[0];
            else
                info = new Reflection.AssemblyInformationalVersionAttribute("");
            
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return Pretty(ver, info.InformationalVersion);
        }

        /// <summary>
        /// Gets the specified version's string with omitting the trailing zeros.
        /// </summary>
        /// <param name="ver">A version</param>
        /// <param name="info">The informal version to suffix the string with</param>
        /// <returns>The version's string prettyfied</returns>
        static public string Pretty(this Version ver, string info = null)
        {
            int fieldCount = 0;

            // Increment the required fields until there is a value (this emits the trailing zeros)
            if (ver.Major != 0)
                fieldCount = 1;
            if (ver.Minor != 0)
                fieldCount = 2;
            if (ver.Build != 0)
                fieldCount = 3;
            if (ver.Revision != 0)
                fieldCount = 4;

            return "v" + ver.ToString(fieldCount) + (!String.IsNullOrWhiteSpace(info) ? "-" + info : String.Empty);
        }
    }
}

namespace System.Cryptography
{
    /// <summary>
    /// Provides methods to compute CRC32 checksums.
    /// </summary>
    static class CRC32
    {
        /// <summary>
        /// The table containing calculation polynomials.
        /// </summary>
        static uint[] table;

        /// <summary>
        /// Calculates the CRC32 checksum for the provided byte array.
        /// </summary>
        /// <param name="bytes">The bytes to calculate the checksum for.</param>
        /// <returns>The checksum as an unsigned integer.</returns>
        public static uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        /// <summary>
        /// Calculates the CRC32 checksum for the provided byte array.
        /// </summary>
        /// <param name="bytes">The bytes to calculate the checksum for.</param>
        /// <returns>The checksum as an array of bytes.</returns>
        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        /// <summary>
        /// Sets up the CRC32 generator by calculating the polynomial values.
        /// </summary>
        static CRC32()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                        temp = (uint)((temp >> 1) ^ poly);
                    else
                        temp >>= 1;
                }
                table[i] = temp;
            }
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
                new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite).Dispose();
                return true; // It is
            }
            catch (Exception)
            {
                // In case any error happens, the file is not writable.
                return false;
            }
        }
    }

    /// <summary>
    /// Provides extension methods for System.IO.Stream instances.
    /// </summary>
    static class StreamExtensions
    {
        /// <summary>
        /// The maximum buffer size to use by the extension methods. This is currently 8 KiB (8192 bytes).
        /// </summary>
        private const int MaxBufferSize = 2 << 12; // 2^13 = 8192, 8 KiB

        /// <summary>
        /// Moves the end part of a Stream from the given position offsetting it by distance.
        /// </summary>
        /// <remarks>CAUTION! Calling this method will byte-by-byte move contents in the file
        /// which might invalidate the file's state, especially if it is a binary file.</remarks>
        /// <param name="stream">The Stream instance on which the move should happen.
        /// This Stream must be readable, writable and seekable.</param>
        /// <param name="position">The position which is the first byte in the Stream. The [position; EOS)
        /// interval of the data will be moved.</param>
        /// <param name="difference">The distance by which the moving part should be moved. It can be negative or positive.</param>
        /// <exception cref="ArgumentException">Thrown if the Stream cannot be read, seeked or written.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The position or difference value was invalid.</exception>
        /// <exception cref="IOException">Thrown if there was an IO Error by working with the Stream.</exception>
        public static void MoveEndPart(this Stream stream, long position, long difference)
        {
            if (!stream.CanRead || !stream.CanSeek || !stream.CanWrite)
                throw new ArgumentException("The stream cannot be read, seeked or written.", "this stream");

            // Move all the bytes from position to the end of the stream to a new position.
            // (In either direction.)
            if (position < 0)
                throw new ArgumentOutOfRangeException("The initial position from where the content should be moved can't be negative.");
            else if (position > stream.Length)
                throw new ArgumentOutOfRangeException("The initial position from where the content should be moved must be inside the stream.");

            if (position == stream.Length && difference < 0)
            {
                // Shrink the stream by the given size
                stream.SetLength(stream.Length + difference); // actually a - :)
                return;
            }

            if (difference == 0 || position == stream.Length)
                return; // Noop, nothing to move.

            if (position + difference < 0)
                throw new ArgumentOutOfRangeException("Requested to move bytes before the beginning of the stream.");

            // First, we calculate how many bytes are there to be moved.
            long fullByteCount = stream.Length - position;

            // This blob is to be chunked up based on MaxBufferSize.
            // For every move operation, a such buffer will be read and written out.
            stream.Seek(position, SeekOrigin.Begin);
            long currentPosition = position;
            long newPosition = position + difference; // Where the moved bytes will begin after the move

            if (fullByteCount == 0)
                return; // Noop, nothing to move.

            // Calculate a buffer size to use
            int bufferSize;
            if (fullByteCount > MaxBufferSize)
                bufferSize = MaxBufferSize;
            else
                bufferSize = (int)Math.Pow(2, Math.Floor(Math.Log(fullByteCount, 2)));
            byte[] buffer = new byte[bufferSize];

            long byteCount = 0; // The count of "done" bytes we already moved
            long readPosition = -1, writePosition = -1; // Two pointers where the next read and write operation will work.

            if (difference > 0)
            {
                // If we are moving FORWARD, the first chunk to be read is the LAST in the file.
                // We start from the right.
                readPosition = stream.Length - bufferSize;
                writePosition = readPosition + difference;

                // Also, if we are moving forward, the stream has to be increased in size.
                stream.SetLength(stream.Length + difference);
            }
            else if (difference < 0)
            {
                // If we are moving BACKWARDS, the first chunk to be read is the FIRST
                // We start from the left.
                readPosition = position;
                writePosition = readPosition + difference; // (well, actually a - here :) )
            }

            int bytesToRead = 0;
            while (byteCount < fullByteCount)
            {
                // If the number of remaining bytes would be smaller than the buffer size, read a partial buffer.
                if (fullByteCount - byteCount < bufferSize)
                    bytesToRead = Convert.ToInt32(fullByteCount - byteCount);
                else
                    bytesToRead = bufferSize;

                // Read the chunk.
                stream.Seek(readPosition, SeekOrigin.Begin);
                stream.Read(buffer, 0, bytesToRead);

                // And write it.
                stream.Seek(writePosition, SeekOrigin.Begin);
                stream.Write(buffer, 0, bytesToRead);
                stream.Flush();

                // Align the two intermediate pointers to the new locations for the next operation.
                // (The read and write positions should always be having a distance of 'difference' between each other.)
                if (difference > 0)
                {
                    // If we are moving the bytes FORWARD, the read head moves BACKWARDS, because we started from the right.

                    // Read and write positions could underflow this way.
                    // If the last remaining chunk is smaller than the buffer and would begin before the initial start position...
                    // we correct it.
                    if (readPosition - bytesToRead < position)
                    {
                        readPosition = position;
                        writePosition = position + difference;
                    }
                    else
                    {
                        readPosition -= bytesToRead;
                        writePosition -= bytesToRead;
                    }
                }
                else if (difference < 0)
                {
                    // If we are moving the bytes BACKWARD, the read and write moves FORWARD, because we started form the left.
                    readPosition += bytesToRead;
                    writePosition += bytesToRead;
                }

                byteCount += bytesToRead; // Mark the currently done bytes... 'done'
            }

            stream.Flush();
            if (difference < 0)
                // If the move operation was to shrink, we eliminate the overhead at the end of the file.
                stream.SetLength(stream.Length + difference); // (still a - :) )
        }
    }
}

#if WINDOWS
namespace SharpGMad
{
    /// <summary>
    /// Provides methods to retrieve information for file associations
    /// </summary>
    static class FileAssocation
    {
        /// <summary>Contains the type name and icons of a file.</summary>
        internal struct TypeAndIcon
        {
            /// <summary>The path of the file</summary>
            public string Filename;
            /// <summary>The file's type string (like: Text Document)</summary>
            public string Type;
            /// <summary>The file's small (16x16 pixel) icon</summary>
            public Icon SmallIcon;
            /// <summary>The file's large (32x32 pixel) icon</summary>
            public Icon LargeIcon;
        }

        /// <summary>
        /// Gets the icon and type name (using a Windows API call) of the file specified.
        /// </summary>
        /// <param name="path">The path of the file we need the icon of.</param>
        public static TypeAndIcon GetInformation(string path)
        {
            // Large icon
            ShFileInfo large = new ShFileInfo();
            ShGetFileInfoAttributes callLarge = ShGetFileInfoAttributes.Icon | ShGetFileInfoAttributes.UseFileAttributes |
                ShGetFileInfoAttributes.LargeIcon | ShGetFileInfoAttributes.SysIconIndex;
            
            ShGetFileInfo(path, (uint)System.IO.FileAttributes.Normal, ref large,
                (uint)Marshal.SizeOf(large), (uint)callLarge);

            // Small icon
            ShFileInfo small = new ShFileInfo();
            ShGetFileInfoAttributes callSmall = ShGetFileInfoAttributes.Icon | ShGetFileInfoAttributes.UseFileAttributes |
                ShGetFileInfoAttributes.SmallIcon | ShGetFileInfoAttributes.SysIconIndex;

            ShGetFileInfo(path, (uint)System.IO.FileAttributes.Normal, ref small,
                (uint)Marshal.SizeOf(small), (uint)callSmall);

            // Typename
            ShFileInfo typename = new ShFileInfo();
            ShGetFileInfoAttributes callTypename = ShGetFileInfoAttributes.UseFileAttributes | ShGetFileInfoAttributes.TypeName;

            ShGetFileInfo(path, (uint)System.IO.FileAttributes.Normal, ref typename,
                (uint)Marshal.SizeOf(typename), (uint)callTypename);


            TypeAndIcon tai = new TypeAndIcon();
            tai.Filename = path;
            tai.Type = typename.szTypeName;

            // Copy the retrieved icon into a local resource to disconnect it from the external one.
            if (large.hIcon != IntPtr.Zero && large.iIcon != 0)
                tai.LargeIcon = (Icon)Icon.FromHandle(large.hIcon).Clone();
            if (small.hIcon != IntPtr.Zero && small.iIcon != 0)
                tai.SmallIcon = (Icon)Icon.FromHandle(small.hIcon).Clone();
            
            DestroyIcon(large.hIcon);
            DestroyIcon(typename.hIcon);
            return tai;
        }

        /// <summary>
        /// Releases the resources of the system icon pointed by the hIcon value.
        /// </summary>
        /// <param name="hIcon">The icon's handle pointer</param>
        /// <returns>Zero if failed, non-zero if successful.</returns>
        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// The implementation of an SHFILEINFO struct containing information about a file.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct ShFileInfo
        {
            /// <summary>
            /// The pointer to the handle of the file's icon.
            /// </summary>
            public IntPtr hIcon;

            /// <summary>
            /// The index of the file's icon within the system image list.
            /// (This seems to be 0 if an item could not be generated for the file.)
            /// </summary>
            public int iIcon;

            /// <summary>
            /// An array of values that indicate attributes of the file.
            /// Unused.
            /// </summary>
            public uint dwAttributes;

            /// <summary>
            /// A string that contains the name of the file as it appears in the Windows Shell,
            /// or the path and file name of the file that contains the icon representing the file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            /// <summary>
            /// A string that describes the type of file. (For example: "Text Document" for .txt files.)
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        /// <summary>
        /// A list of attributes that can be used to fine tune ShGetFileInfo().
        /// </summary>
        [Flags]
        private enum ShGetFileInfoAttributes : uint
        {
            /// <summary>Populate ShFileInfo.hIcon with the file's icon's handle and ShFileInfo.iIcon with its index.</summary>
            Icon = 0x000000100,
            /// <summary>Put the file's display name into ShFileInfo.szDisplayName.</summary>
            //DisplayName = 0x000000200,
            /// <summary>Retrieve the file's type string in ShFileInfo.szTypeName.</summary>
            TypeName = 0x000000400,
            /// <summary>Retrieve the item's attributes into ShFileInfo.dwAttributes.</summary>
            //Attributes = 0x000000800,
            /// <summary>Get the name of the file where the icon is stored (for example: Notepad.exe for .txt files) into ShFileInfo.szDisplayName
            /// This will also retrieve the icon's index into ShFileInfo.iIcon.</summary>
            //IconLocation = 0x000001000,
            /// <summary>Get the type of executable if the file is an executable. (?)</summary>
            //ExeType = 0x000002000,
            /// <summary>Retrieves the index of a system image list icon. If successful, ShFileInfo.iIcon is populated with the value.</summary>
            SysIconIndex = 0x000004000,
            /// <summary>Adds the little file link arrow overlaid on the icon if Icon is set.</summary>
            //LinkOverlay = 0x000008000,
            /// <summary>If Icon is set, the returned icon will be blended with the highlight colour as if it would be selected.</summary>
            //Selected = 0x000010000,
            /// <summary>Indicate that the ShFileInfo.dwAttributes contain specific attributes. (?)</summary>
            //Attr_Specified = 0x000020000,
            /// <summary>If Icon is set, the retrieved icon will be a large (32x32) icon.</summary>
            LargeIcon = 0x000000000,
            /// <summary>If Icon is set, the retrieved icon will be a small (16x16) icon.
            /// If SysIconIndex is set, the call will return a handle to the system image list of small icons. (?)</summary>
            SmallIcon = 0x000000001,
            /// <summary>If Icon is set, the file's open icon will be retrieved.
            /// (like ZIP files? AFAIK, only folders have open icons :D)
            /// If SysIconIndex is set, the call will return the handle to the system image list. (?)</summary>
            //OpenIcon = 0x000000002,
            /// <summary>If Icon is set, a Shell-sized icon will be retrieved. (?)</summary>
            //ShellIconSize = 0x000000004,
            /// <summary>The file path passed is not a file path, but a pointer to an ItemIDList. (?)</summary>
            //PIDL = 0x000000008,
            /// <summary>If set, the call will not attempt to access the file at the specified path.
            /// Instead, it will act like the file path given exists with the attributes given in dwFileAttributes.</summary>
            UseFileAttributes = 0x000000010,
            /// <summary>Whether certain overlays (like TortoiseSVN's status icons?) should be applied to the icon.</summary>
            //AddOverlays = 0x000000020,
            /// <summary>If Icon is set, call returns the value of the overlaid icon's index in the upper eight bits of iIcon.</summary>
            //OverlayIndex = 0x000000040,
        }

        /// <summary>
        /// Calls the external SHGetFileInfo method of Shell32.dll to retrieve information about a file.
        /// </summary>
        /// <param name="pszPath">The file to retrieve information about.</param>
        /// <param name="dwFileAttributes">File attribute flags (?, unused)</param>
        /// <param name="psfi">The structure where certain values will be populated accordingly.</param>
        /// <param name="cbFileInfo">The size of the structure passed at psfi.</param>
        /// <param name="uFlags">Flags from SHGetFileInfoAttributes fine-tuning the behaviour of the call.</param>
        /// <returns>Some sort of a pointer... (?)</returns>
        [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr ShGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi,
            uint cbFileInfo, uint uFlags);
    }
}
#endif