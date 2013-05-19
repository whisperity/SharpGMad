using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGMad
{
    /// <summary>
    /// Represents an error with files checking against ignore list.
    /// </summary>
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

    /// <summary>
    /// Represents an addon file in the program's memory and provides methods to manipulate an addon.
    /// </summary>
    class Addon
    {
        /// <summary>
        /// The identation (first few characters) of GMA files.
        /// </summary>
        public const string Ident = "GMAD";
        /// <summary>
        /// The version byte for GMA files.
        /// </summary>
        public const char Version = (char)3;
        /* (Currently not used)
        /// <summary>
        /// The AppID of Garry's Mod on Steam.
        /// </summary>
        public const uint AppID = 4000;*/
        /* (Currently not used.)
        /// <summary>
        /// Some sort of checksum/signature entry.
        /// </summary>
        public const uint CompressionSignature = 0xBEEFCACE;*/
        
        /* (Currently not used.)
        /// <summary>
        /// Represents the header of a GMA file.
        /// </summary>
        private struct Header
        {
            /// <summary>
            /// The identation (first few characters).
            /// </summary>
            public string Ident;
            /// <summary>
            /// The version byte.
            /// </summary>
            public char Version;

            /// <summary>
            /// Creates a new header using the identation and version specified.
            /// </summary>
            /// <param name="ident">The identation (first few characters)</param>
            /// <param name="version">The version byte.</param>
            public Header(string ident, char version)
            {
                this.Ident = ident;
                this.Version = version;
            }
        }*/

        /* (Current not used.)
        /// <summary>
        /// This is the position in the file containing a 64 bit unsigned int that represents the file's age
        /// It's basically the time it was uploaded to Steam - and is set on download/extraction from steam.
        /// </summary>
        public static uint TimestampOffset = (uint)System.Runtime.InteropServices.Marshal.SizeOf(new Header(Ident, Version))
            + (uint)sizeof(ulong);*/
        
        /// <summary>
        /// Gets or sets the title of the addon.
        /// </summary>
        public string Title;
        /// <summary>
        /// Gets or sets the author of the addon.
        /// Currently it has no use, as the author is always written as "Author Name".
        /// </summary>
        public string Author; // Not used. Current writer only writes "Author Name"
        /// <summary>
        /// Gets or sets the description of the addon.
        /// </summary>
        public string Description;
        /// <summary>
        /// Gets a compiled JSON string of the addon's metadata.
        /// </summary>
        public string DescriptionJSON { get { return Json.BuildDescription(this); } }
        /// <summary>
        /// Gets or sets the type of the addon.
        /// </summary>
        public string Type;
        /// <summary>
        /// Contains a list of file contents of the addon.
        /// </summary>
        private List<ContentFile> _Files;
        /// <summary>
        /// Gets a list of files and contents currently added to the addon.
        /// </summary>
        public List<ContentFile> Files { get { return new List<ContentFile>(_Files); } }
        /// <summary>
        /// Gets or sets a list of addon tags.
        /// </summary>
        public List<string> Tags;
        /// <summary>
        /// Gets or sets a list of addon ignore patterns.
        /// </summary>
        public List<string> Ignores;

        /// <summary>
        /// Initializes an new, empty Addon instance.
        /// </summary>
        public Addon()
        {
            _Files = new List<ContentFile>();
            Tags = new List<string>();
            Ignores = new List<string>();
        }

        /// <summary>
        /// Sets up a new instance of Addon using the data provided by the specified addon reader.
        /// </summary>
        /// <param name="reader">The addon reader which handled reading the addon file.</param>
        public Addon(Reader reader)
            : this()
        {
            Author = reader.Author;
            Title = reader.Name;
            Description = reader.Description;
            Type = reader.Type;
            Tags = reader.Tags;

            foreach (Reader.IndexEntry file in reader.Index)
            {
                MemoryStream buffer = new MemoryStream();
                reader.GetFile(file.FileNumber, buffer);
                
                buffer.Seek(0, SeekOrigin.Begin);

                byte[] bytes = new byte[buffer.Length];
                buffer.Read(bytes, 0, (int)buffer.Length);

                AddFile(file.Path, bytes);
            }
        }

        /// <summary>
        /// Sets up a new instance of Addon using the metadata provided from the specified JSON.
        /// </summary>
        /// <param name="addonJson">The JSON instance containing the metadata to use.</param>
        public Addon(Json addonJson)
            : this()
        {
            Title = addonJson.Title;
            Description = addonJson.Description;
            Type = addonJson.Type;
            Tags = new List<string>(addonJson.Tags);
            Ignores = new List<string>(addonJson.Ignores);
        }
        
        /// <summary>
        /// Gets whether the specified file is ignored by the addon's ignore list.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>True if the addon is ignored, false if not.</returns>
        private bool IsIgnored(string path)
        {
            foreach (string pattern in Ignores)
                if (Whitelist.Check(pattern, path)) return true;

            return false;
        }

        /// <summary>
        /// Gets whether the specified file is allowed to be in GMAs by the global whitelist.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>True if the addon is allowed, false if not.</returns>
        private bool IsWhitelisted(string path)
        {
            return Whitelist.Check(path.ToLowerInvariant());
        }

        /// <summary>
        /// Adds the specified file into the Addon's internal container.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="content">Array of bytes containing the file content.</param>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        /// <exception cref="IgnoredException">The file is prohibited from storing by the addon's ignore list.</exception>
        public void AddFile(string path, byte[] content)
        {
            if (path.ToLowerInvariant() != path)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t\t[Filename contains capital letters]");
                Console.ResetColor();
                path = path.ToLowerInvariant();
            }

            ContentFile file = new ContentFile();
            file.Content = content;
            file.Path = path;

            // Check if file is ignored
            if (path == "addon.json")
                return;
            if (IsIgnored(path))
                throw new IgnoredException(path + ": ignored");
            if (!IsWhitelisted(path))
                throw new WhitelistException(path + ": not allowed by whitelist.");

            if ( !IsIgnored(path) && IsWhitelisted(path) )
                _Files.Add(file);
        }

        /// <summary>
        /// Sorts the internal file list by filename.
        /// </summary>
        public void Sort()
        {
            _Files.Sort((x, y) => String.Compare(x.Path, y.Path));
        }

        /// <summary>
        /// Removes the specified file and its contents from the internal storage.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <exception cref="FileNotFoundException">The specified file is not in the collection.</exception>
        public void RemoveFile(string path)
        {
            IEnumerable<ContentFile> toRemove = _Files.Where(e => e.Path == path);

            if (toRemove.Count() == 0)
                throw new FileNotFoundException("The file is not in the archive.");
            else if (toRemove.Count() == 1)
                _Files.Remove(toRemove.First());
        }
    }

    /// <summary>
    /// Represents a file entry to an Addon instance.
    /// </summary>
    struct ContentFile
    {
        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        public string Path;
        /// <summary>
        /// Gets or sets an array of bytes containg the file content.
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// Gets the size of the content.
        /// </summary>
        public long Size { get { return Content.LongLength; } }
        /// <summary>
        /// Gets the CRC32 checksum of the content.
        /// </summary>
        public ulong CRC { get { return Crc32.ComputeChecksum(Content); } }
    }
}
