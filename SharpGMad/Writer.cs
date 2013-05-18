using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    static class Writer
    {
        public static void Create(Addon addon, out MemoryStream outBuffer)
        {
            outBuffer = new MemoryStream();

            using ( MemoryStream buffer = new MemoryStream() )
            using (BinaryWriter writer = new BinaryWriter(buffer))
            {
                // Header (5)
                writer.Write(Addon.Ident.ToCharArray()); // Ident (4)
                writer.Write((char)Addon.Version); // Version (1)
                // SteamID (8) [unused]
                writer.Write((ulong)0);
                // TimeStamp (8)
                writer.Write((ulong)
                    (((TimeSpan)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime())).TotalSeconds));
                // Required content (a list of strings)
                writer.Write((char)0); // signifies nothing
                // Addon Name (n)
                writer.WriteNullTerminatedString(addon.Title);
                // Addon Description (n)
                writer.WriteNullTerminatedString(addon.DescriptionJSON);
                // Addon Author (n) [unused]
                writer.WriteNullTerminatedString("Author Name");
                // Addon v ersion (4) [unused]
                writer.Write((int)1);

                // File list
                uint fileNum = 0;

                foreach (KeyValuePair<string, byte[]> f in addon.Files.ToDictionary(k => k.Path, e => e.Content))
                {
                    // Remove prefix / from filename
                    string file = f.Key.TrimStart('/');

                    uint crc = Crc32.ComputeChecksum(f.Value); // unsigned long
                    long size = (long)f.Value.Length; // long long
                    fileNum++;

                    writer.Write(fileNum); // File number (4)
                    writer.WriteNullTerminatedString(file.ToLowerInvariant()); // File name (all lower case!) (n)
                    writer.Write(size); // File size (8)
                    writer.Write(crc); // File CRC (4)
                    Console.WriteLine("File index: " + file + " [CRC: " + crc + "] [Size:" + Program.Memory((int)size) + "]");
                }

                // Zero to signify the end of files
                fileNum = 0;
                writer.Write(fileNum);

                // The files
                foreach (KeyValuePair<string, byte[]> f in addon.Files.ToDictionary(k => k.Path, e => e.Content))
                {
                    // Remove prefix / from filename
                    string file = f.Key.TrimStart('/');

                    Console.WriteLine("Adding " + file);
                    writer.Write(f.Value);
                }

                // CRC what we've written (to verify that the download isn't shitted) (4)
                writer.Seek(0, SeekOrigin.Begin);
                byte[] buffer_whole = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(buffer_whole, 0, (int)writer.BaseStream.Length);
                ulong addonCRC = Crc32.ComputeChecksum(buffer_whole);
                writer.Write(addonCRC);

                writer.BaseStream.Seek(0, SeekOrigin.Begin);
                buffer.CopyTo(outBuffer);
            }
        }
    }
}
