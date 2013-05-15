using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Addon;

namespace SharpGMad
{
    class UpdatableAddon
    {
        public struct ContentFile
        {
            public string Path;
            public uint CRC;
            public byte[] Content;
            public long Size { get { return Content.Length; } }
        }

        public string Title;
        public string Author;
        public string Description;
        public string Type;
        public List<ContentFile> Files;
        //char FormatVersion;
        public List<string> Tags;

        public MemoryStream Buffer;

        public UpdatableAddon()
        {
            Title = "";
            Author = "";
            Description = "";
            Type = "";
            //FormatVersion = Addon.Format.Version;
            Files = new List<ContentFile>();
            Tags = new List<string>();
            Buffer = new MemoryStream();
        }

        public void AddFile(string strName, byte[] bContent)
        {
            Crc32 crc32 = new Crc32();

            ContentFile content = new ContentFile();
            content.Content = bContent;
            content.Path = strName;
            content.CRC = crc32.ComputeChecksum(bContent);

            Files.Add(content);
        }

        public void RemoveFile(string strName)
        {
            ContentFile toRemove = Files.Where(e => e.Path == strName).First();
            Files.Remove(toRemove);
        }

        public void UpdateInternalStream()
        {
            //
            // Load the Addon Info file
            //
            Addon.Json addonInfo = new Addon.Json(Title, Description, Type, Tags, new List<string>());

            //
            // Get a list of files in the specified folder
            //
            List<string> files = Files.Select(s => s.Path).ToList();
            //
            // Let the addon json remove the ignored files
            //
            addonInfo.RemoveIgnoredFiles(ref files);
            //
            // Sort the list into alphabetical order, no real reason - we're just ODC
            //
            //files.Sort();

            //
            // Verify
            //
            if (!CreateAddon.VerifyFiles(ref files, true))
            {
                Output.Warning("File list verification failed");
                return;
            }

            //
            // Create an addon file in a buffer
            //
            Dictionary<string, byte[]> fileContents = Files.ToDictionary(k => k.Path, e => e.Content);
            Writer writer;
            try
            {
                writer = new Writer(Title, addonInfo.BuildDescription(), fileContents);
            }
            catch (Exception)
            {
                Output.Warning("Failed to create the addon");
                return;
            }

            //
            // Save the buffer to the provided name
            //

            //
            // Success!
            //
            Console.WriteLine("Successfully saved to \"internal buffer\" [" + Program.Memory((int)Buffer.Length) + "]");
            return;
        }
    }
}
