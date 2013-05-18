using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    class UpdatableAddon
    {
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
            ContentFile content = new ContentFile();
            content.Content = bContent;
            content.Path = strName;

            Files.Add(content);
        }

        public void RemoveFile(string strName)
        {
            IEnumerable<ContentFile> toRemove = Files.Where(e => e.Path == strName);

            if (toRemove.Count() == 0)
            {
                Output.Warning("The file is not in the archive.");
            }
            else if (toRemove.Count() == 1)
            {
                Files.Remove(toRemove.First());
                Console.WriteLine("File removed.");
            }
            else
            {
                Output.Warning("Ambigous argument. More than one file matches.");
                foreach (ContentFile f in toRemove)
                    Console.WriteLine(f.Path);
            }
        }

        public void UpdateInternalStream()
        {
            //
            // Load the Addon Info file
            //
            Json addonInfo = new Json(Title, Description, Type, Tags, new List<string>());

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
                Buffer = writer.Get();
            }
            catch (Exception e)
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
