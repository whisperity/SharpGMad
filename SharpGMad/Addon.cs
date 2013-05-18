using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    [Serializable]
    class WhitelistException : Exception
    {
        public WhitelistException() { }
        public WhitelistException(string message) : base(message) { }
        public WhitelistException(string message, Exception inner) : base(message, inner) { }
        protected WhitelistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    class IgnoredException : Exception
    {
        public IgnoredException() { }
        public IgnoredException(string message) : base(message) { }
        public IgnoredException(string message, Exception inner) : base(message, inner) { }
        protected IgnoredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    class Addon
    {
        // Static members: format setup.
        public const string Ident = "GMAD";
        public const char Version = (char)3;
        public const uint AppID = 4000;
        public const uint CompressionSignature = 0xBEEFCACE;

        private struct Header
        {
            public string Ident;
            public char Version;

            public Header(string ident, char version)
            {
                this.Ident = ident;
                this.Version = version;
            }
        }

        //
        // This is the position in the file containing a 64 bit unsigned int that represents the file's age
        // It's basically the time it was uploaded to Steam - and is set on download/extraction from steam.
        //
        public static uint TimestampOffset = (uint)System.Runtime.InteropServices.Marshal.SizeOf(new Header(Ident, Version))
            + (uint)sizeof(ulong);

        // Instance members
        public string Title;
        public string Author;
        public string Description;
        public string DescriptionJSON { get { return Json.BuildDescription(this); } }
        public string Type;
        private List<ContentFile> _Files;
        public List<ContentFile> Files { get { return new List<ContentFile>(_Files); } }
        public List<string> Tags;
        public List<string> Ignores;

        private Addon()
        {
            _Files = new List<ContentFile>();
            Tags = new List<string>();
            Ignores = new List<string>();
        }

        public Addon(Reader reader)
            : this()
        {
            Author = reader.Author;
            Title = reader.Name;
            Description = reader.Description;
            Type = reader.Type;
            Tags = reader.Tags;

            Console.WriteLine("Loaded addon " + Title);
            Console.WriteLine("Loading files from GMA...");

            foreach (Reader.FileEntry file in reader.Index)
            {
                MemoryStream buffer = new MemoryStream();
                reader.GetFile(file.FileNumber, buffer);
                
                buffer.Seek(0, SeekOrigin.Begin);

                byte[] bytes = new byte[buffer.Length];
                buffer.Read(bytes, 0, (int)buffer.Length);

                AddFile(file.Path, bytes);

                Console.WriteLine(file.Path + " loaded.");
            }

            Console.WriteLine("Addon opened successfully.");
        }

        public Addon(Json addonJson) : this()
        {
            Title = addonJson.Title;
            Description = addonJson.Description;
            Type = addonJson.Type;
            Tags = new List<string>(addonJson.Tags);
            Ignores = new List<string>(addonJson.Ignores);
        }

        

        private bool IsIgnored(string path)
        {
            if (path == "addon.json") return true;

            foreach (string pattern in Ignores)
                if (Whitelist.TestWildcard(pattern, path)) return true;

            return false;
        }

        private bool IsWhitelisted(string path)
        {
            return Whitelist.Check(path.ToLowerInvariant());
        }

        public void AddFile(string path, byte[] content)
        {
            if (path.ToLowerInvariant() != path)
            {
                Output.Warning("\t\t[Filename contains capital letters]");
                path = path.ToLowerInvariant();
            }

            ContentFile file = new ContentFile();
            file.Content = content;
            file.Path = path;

            // Check if file is ignored
            if (IsIgnored(path))
                throw new IgnoredException(path + ": ignored");
            if (!IsWhitelisted(path))
                throw new WhitelistException(path + ": not allowed by whitelist.");

            if ( !IsIgnored(path) && IsWhitelisted(path) )
                _Files.Add(file);
        }

        public void Sort()
        {
            _Files.Sort((x, y) => String.Compare(x.Path, y.Path));
        }

        public void RemoveFile(string path)
        {
            IEnumerable<ContentFile> toRemove = _Files.Where(e => e.Path == path);

            if (toRemove.Count() == 0)
            {
                Output.Warning("The file is not in the archive.");
            }
            else if (toRemove.Count() == 1)
            {
                _Files.Remove(toRemove.First());
                Console.WriteLine("File removed.");
            }
            else
            {
                Output.Warning("Ambigous argument. More than one file matches.");
                foreach (ContentFile f in toRemove)
                    Console.WriteLine(f.Path);
            }
        }
    }

    struct ContentFile
    {
        public string Path;
        public byte[] Content;

        public long Size { get { return Content.LongLength; } }
        public ulong CRC { get { return Crc32.ComputeChecksum(Content); } }
    }

    static class Tags
    {
        // Only one of these
        private static string[] Type = new string[]{
            "gamemode",
            "map",
            "weapon",
            "vehicle",
            "npc",
            "tool",
            "effects",
            "model"
        };

        public static bool TypeExists(string type) { return Type.Contains(type); }

        // Up to two of these
        private static string[] Misc = new string[]{
            "fun",
            "roleplay",
            "scenic",
            "movie",
            "realism",
            "cartoon",
            "water",
            "comic",
            "build"
        };

        public static bool TagExists(string tag) { return Misc.Contains(tag); }
    }
}
