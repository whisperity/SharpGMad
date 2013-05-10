using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    class CreateAddon
    {
        static public bool VerifyFiles(ref List<string> files, bool warnInvalid)
        {
            bool bOk = true;

            //
            // Bail out if there's no files
            //
            if (files.Count == 0)
            {
                Console.WriteLine("No files found, can't continue!");
                bOk = false;
            }

            List<string> old_files = new List<string>(files);
            files.Clear();
            //
            // Print each found file, check they're ok
            //
            foreach (string file in old_files)
            {
                Console.WriteLine("\t" + file);

                //
                // Check the file against the whitelist
                // Lowercase the name (addon filesystem is case insentive)
                //
                if (Addon.Whitelist.Check(file.ToLowerInvariant()))
                    files.Add(file);
                else
                {
                    Output.Warning("\t\t[Not allowed by whitelist]");
                    if (!warnInvalid)
                        bOk = false;
                }

                //
                // Warn that we're gonna lowercase the filename
                if (file.ToLowerInvariant() != file)
                {
                    Output.Warning("\t\t[Filename contains capital letters]");
                }
            }
            return bOk;
        }

        static private void WriteStringNULDelimiter(BinaryWriter wr, string str)
        {
            wr.Write(Encoding.ASCII.GetBytes(str));
            wr.Write((byte)0x00);
        }

        //
        // Create an uncompressed GMAD file from a list of files
        //
        static public bool Create(ref MemoryStream buffer, string strFolder, ref List<string> files, string strTitle, string strDescription)
        {
            // Remove / (if exists) and then purposely add it back
            // Ensure that there is a tailing /
            strFolder = strFolder.TrimEnd('/');
            strFolder = strFolder + '/';

            BinaryWriter writer = new BinaryWriter(buffer);

            // Header (5)
            writer.Write(Addon.Format.Ident.ToCharArray()); // Ident (4)
            writer.Write((char)Addon.Format.Version); // Version (1)
            // SteamID (8) [unused]
            writer.Write((ulong)0);
            // TimeStamp (8)
            writer.Write((ulong)(((TimeSpan)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime())).TotalSeconds));
            // Required content (a list of strings)
            writer.Write((char)0); // signifies nothing
            // Addon Name (n)
            WriteStringNULDelimiter(writer, strTitle);
            // Addon Description (n)
            WriteStringNULDelimiter(writer, strDescription);
            // Addon Author (n) [unused]
            WriteStringNULDelimiter(writer, "Author Name");
            // Addon Version (4) [unused]
            writer.Write((int)1);
            // File list
            uint iFileNum = 0;

            Crc32 crc32 = new Crc32();

            foreach (string f in files)
            {
                // Remove prefix / from filename
                string file = f.TrimStart('/');

                uint iCRC = crc32.ComputeChecksum(File.ReadAllBytes(strFolder + file)); // unsigned long
                FileInfo fi = new FileInfo(strFolder + file);
                long iSize = fi.Length; // long long
                iFileNum++;
                writer.Write((uint)iFileNum); // File number (4)
                WriteStringNULDelimiter(writer, file.ToLowerInvariant()); // File name (all lower case!) (n)
                writer.Write((long)iSize); // File size (8)
                writer.Write((uint)iCRC); // File CRC (4)
                Console.WriteLine("File index: " + file + " [CRC:" + iCRC + "]" +
                    " [Size:" + Program.Memory((int)iSize) + "]");
            }
            // Zero to signify end of files
            iFileNum = 0;
            writer.Write((uint)iFileNum);
            // The files
            foreach (string f in files)
            {
                // Remove prefix / from filename
                string file = f.TrimStart('/');

                Console.WriteLine("Adding " + file);

                FileInfo fi = new FileInfo(strFolder + file);

                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        ms.Write(File.ReadAllBytes(strFolder + file), 0, (int)fi.Length);

                        if (ms.Length == 0)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        Output.Warning("File " + strFolder + file + " seems to be empty (or we couldn't read it)");

                        return false;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] ms_buffer = new byte[ms.Length];
                    ms.Read(ms_buffer, 0, (int)ms.Length);

                    writer.Write(ms_buffer, 0, ms_buffer.Length);
                }
            }
            // CRC what we've written (to verify that the download isn't shitted) (4)
            writer.Seek(0, SeekOrigin.Begin);
            byte[] buffer_whole = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(buffer_whole, 0, (int)writer.BaseStream.Length);
            ulong AddonCRC = crc32.ComputeChecksum(buffer_whole);
            writer.Write(AddonCRC);

            buffer.Seek(0, SeekOrigin.Begin);

            return true;
        }
    }
}
