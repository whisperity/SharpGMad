using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    class Writer
    {
        private MemoryStream Buffer;

        private Writer()
        {
            Buffer = new MemoryStream();
        }

        public Writer(string title, string descriptionJson, string folder, List<string> filePaths) : this()
        {
            Header(title, descriptionJson);
            Files(folder, filePaths);
            Footer();
        }

        public Writer(string title, string descriptionJson, Dictionary<string, byte[]> fileContents) : this()
        {
            Header(title, descriptionJson);
            Files(fileContents);
            Footer();
        }

        private void Header(string title, string descriptionJson)
        {
            BinaryWriter writer = new BinaryWriter(Buffer);

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
            writer.WriteNullTerminatedString(title);
            // Addon Description (n)
            writer.WriteNullTerminatedString(descriptionJson);
            // Addon Author (n) [unused]
            writer.WriteNullTerminatedString("Author Name");
            // Addon v ersion (4) [unused]
            writer.Write((int)1);
        }

        private void Files(Dictionary<string, byte[]> fileContents)
        {
            BinaryWriter writer = new BinaryWriter(Buffer);

            // File list
            uint fileNum = 0;

            foreach (KeyValuePair<string, byte[]> f in fileContents)
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
            foreach (KeyValuePair<string, byte[]> f in fileContents)
            {
                // Remove prefix / from filename
                string file = f.Key.TrimStart('/');

                Console.WriteLine("Adding " + file);
                writer.Write(f.Value);
            }
        }

        private void Files(string folder, List<string> files)
        {
            BinaryWriter writer = new BinaryWriter(Buffer);

            // File list
            uint fileNum = 0;

            foreach (string f in files)
            {
                // Remove prefix / from filename
                string file = f.TrimStart('/');
                uint crc;

                try
                {
                    crc = Crc32.ComputeChecksum(File.ReadAllBytes(folder + file)); // unsigned long
                }
                catch (Exception ex)
                {
                    Output.Warning("Was unable to read file. " + folder + file);
                    throw new Exception("Unable to read file " + folder + file, ex);
                }

                FileInfo fi = new FileInfo(folder + file);
                long size = fi.Length; // long long
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
            foreach (string f in files)
            {
                // Remove prefix / from filename
                string file = f.TrimStart('/');

                Console.WriteLine("Adding " + file);

                using (MemoryStream stream = new MemoryStream())
                {
                    FileInfo fi = new FileInfo(folder + file);

                    try
                    {
                        stream.Write(File.ReadAllBytes(folder + file), 0, (int)fi.Length);

                        if (stream.Length == 0)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        Output.Warning("File " + file + " seems to be empty (or we couldn't read it)");

                        return;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] ms_buffer = new byte[stream.Length];
                    stream.Read(ms_buffer, 0, (int)stream.Length);

                    writer.Write(ms_buffer, 0, ms_buffer.Length);
                }
            }
        }

        private void Footer()
        {
            BinaryWriter writer = new BinaryWriter(Buffer);

            // CRC what we've written (to verify that the download isn't shitted) (4)
            writer.Seek(0, SeekOrigin.Begin);
            byte[] buffer_whole = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(buffer_whole, 0, (int)writer.BaseStream.Length);
            ulong addonCRC = Crc32.ComputeChecksum(buffer_whole);
            writer.Write(addonCRC);

            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            return;
        }

        public MemoryStream Get()
        {
            MemoryStream output = new MemoryStream();
            Buffer.Seek(0, SeekOrigin.Begin);
            Buffer.CopyTo(output);

            return output;
        }
    }
}
