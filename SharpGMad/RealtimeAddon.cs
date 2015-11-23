using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGMad
{
    /// <summary>
    /// Represents an error regarding reading addon files.
    /// </summary>
    [Serializable]
    class ReaderException : Exception
    {
        public ReaderException() { }
        public ReaderException(string message) : base(message) { }
        public ReaderException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Encapsulates an Addon and provides the extended "realtime" functionality over it.
    /// </summary>
    class RealtimeAddon
    {
        #region Addon header information
        /// <summary>
        /// The identification (first few characters) of GMA files
        /// </summary>
        public const string Ident = "GMAD";
        /// <summary>
        /// The version byte for GMA files
        /// </summary>
        public const char Version = (char)3;
        /* (Currently not used)
        /// <summary>
        /// The AppID of Garry's Mod on Steam
        /// </summary>
        public const uint AppID = 4000;*/
        /* (Currently not used.)
        /// <summary>
        /// Some sort of checksum/signature entry
        /// </summary>
        public const uint CompressionSignature = 0xBEEFCACE;*/

        /* (Currently not used.)
        /// <summary>
        /// Represents the header of a GMA file
        /// </summary>
        private struct Header
        {
            /// <summary>
            /// The identation (first few characters)
            /// </summary>
            public string Ident;
            /// <summary>
            /// The version byte
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
        #endregion

        #region Metada fields
        private string _Title;
        private char _FormatVersion;
        //private string _Author;
        private string _Description;
        private string _Type;
        //private ulong _SteamID;
        private List<string> _Tags;
        private DateTime _Timestamp;
        #endregion

        /// <summary>
        /// Gets the format version byte of the addon (from header).
        /// </summary>
        public char FormatVersion { get; private set; }

        #region Metadata accessors
        /// <summary>
        /// Gets the title of the addon
        /// </summary>
        public string Title { get { return _Title; } set { if (value != _Title) { _Title = value; Modified = true; } } }
        /*/// <summary>
        /// Gets or sets the author of the addon
        /// Currently it has no use, as the author is always written as "Author Name".
        /// </summary>
        public string Author { get; set; } // Not used. Current writer only writes "Author Name"*/
        /// <summary>
        /// Gets the description of the addon
        /// </summary>
        public string Description { get { return _Description; } set { if (value != _Description) { _Description = value; Modified = true; } } }
        /// <summary>
        /// Gets the type of the addon
        /// </summary>
        public string Type { get { return _Type; } set { if (value != _Type) { _Type = value; Modified = true; } } }
        /* Not used.
        /// <summary>
        /// Gets or sets the Steam ID of the creator
        /// </summary>
        public ulong SteamID  { get { return _SteamID; } set { if (value != _SteamID) { _SteamID = value; Modified = true; } } }*/
        /// <summary>
        /// Gets the creation time of the addon
        /// </summary>
        public DateTime Timestamp { get { return _Timestamp; } set { if (value != _Timestamp) { _Timestamp = value; Modified = true; } } }
        /// <summary>
        /// Gets a list of tags for the addon. To set tags, please use the SetTag() method!
        /// </summary>
        public string[] Tags
        {
            get
            {
                string[] retVal = new string[_Tags.Count];
                Array.Copy(_Tags.ToArray(), retVal, retVal.Length);

                return retVal;
            }
        }

        /// <summary>
        /// Sets one of the tags of the addon.
        /// </summary>
        /// <param name="id">The index of the tag to set. Can be the number 0 or 1 to set the first or second tag.</param>
        /// <param name="tag">The tag to use, it must be a valid tag from Tags.Misc.</param>
        /// <exception cref="ArgumentException">Is thrown when one of the arguments are improper.</exception>
        public void SetTag(int id, string tag)
        {
            if (id < 0 || id > 1)
                throw new ArgumentException("An addon can only have two tags!", "id");

            if (!SharpGMad.Tags.TagExists(tag))
                throw new ArgumentException("The specified tag is not valid.", "tag");

            if (_Tags.Count == 0 && id == 0)
                _Tags.Add(tag);
            else if (_Tags.Count == 1 && id == 1)
                _Tags.Add(tag);
            else
                _Tags[id] = tag;

            Modified = true;
        }
        #endregion

        /// <summary>
        /// The offset in the file where the file index begins
        /// </summary>
        public ulong IndexBlock { get; protected set; }
        /// <summary>
        /// The offset where the file contents begin in the file
        /// </summary>
        public ulong FileBlock { get; protected set; }

        /// <summary>
        /// The stream handle of the current open addon.
        /// </summary>
        private FileStream AddonStream;
        /// <summary>
        /// Gets whether the Stream of the encapsulated Addon is writable.
        /// </summary>
        public bool CanWrite { get { return AddonStream.CanWrite; } }
        /// <summary>
        /// Gets the file path of the addon on the local filesystem.
        /// </summary>
        public string AddonPath { get { return AddonStream.Name; } }
        /// <summary>
        /// Stores the files of the addon
        /// </summary>
        private List<ContentFile> Files;
        
        /// <summary>
        /// Get the collection of files stored in the current realtime addon
        /// </summary>
        public IEnumerable<ContentFile> GetFiles()
        {
            foreach (ContentFile file in Files)
            {
                yield return file;
            }
        }


        /// <summary>
        /// Indicates whether the current addon is modified (the state in memory differs from the state of the filestream).
        /// </summary>
        private bool _Modified;
        /// <summary>
        /// Gets whether the current addon is modified (the state in memory differs from the state of the filestream).
        /// It can also set the modified state to true.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if external assembly attempts to set value to false
        /// or attempts to modify a read-only addon.</exception>
        public bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                if (value)
                {
                    if (!CanWrite)
                    {
                        throw new InvalidOperationException("Unable to modify a read-only addon.");
                    }

                    _Modified = value;
                }
                else if (!value)
                {
                    throw new InvalidOperationException("The modified state cannot be set to false externally.");
                }
            }
        }
        /// <summary>
        /// Gets whether there are changed exported files.
        /// </summary>
        public bool Pullable { get { return WatchedFiles.Any(fw => fw.Modified == true); } }
        /// <summary>
        /// Contains exported, currently being watched files.
        /// </summary>
        public List<FileWatch> WatchedFiles { get; private set; }
        
        /// <summary>
        /// Loads the specified addon from the local filesystem and encapsulates it within a realtime instance.
        /// </summary>
        /// <param name="filename">The path to the file on the local filesystem.</param>
        /// <param name="readOnly">True if the file is to be opened read-only, false otherwise</param>
        /// <returns>A RealtimeAddon instance.</returns>
        /// <exception cref="FileNotFoundException">Happens if the specified file does not exist.</exception>
        /// <exception cref="IOException">Thrown if there is a problem opening the specified file.</exception>
        /// <exception cref="ReaderException">Thrown if the addon reader and parser encounters an error.</exception>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">There is a file prohibited from storing by the global whitelist.</exception>
        public static RealtimeAddon Load(string filename, bool readOnly = false)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }

            FileStream fs = null;
            try
            {
                if (!readOnly)
                    fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                else
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                if (fs != null)
                    fs.Dispose();

                throw;
            }

            RealtimeAddon realtime;
            try
            {
                if (!fs.CanRead || !fs.CanSeek)
                    throw new ArgumentException("Cannot create an addon from an unreadable or unseekable Stream.");

                // Check if the Stream can really be seeked and read
                try
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.ReadByte();
                    fs.Seek(0, SeekOrigin.Begin);
                }
                catch (IOException)
                {
                    throw;
                }

                // Read the addon
                try
                {
                    realtime = new RealtimeAddon(fs);
                }
                catch (IOException)
                {
                    throw;
                }
                catch (ReaderException)
                {
                    throw;
                }
            }
            catch (ArgumentException)
            {
                fs.Dispose();
                throw;
            }
            catch (WhitelistException)
            {
                fs.Dispose();
                throw;
            }
            
            return realtime;
        }

        /// <summary>
        /// Creates a new, empty addon and encapsulates it within a realtime instance.
        /// </summary>
        /// <param name="filename">The path of the addon file to create.</param>
        /// <returns>A RealtimeAddon instance.</returns>
        /// <exception cref="UnauthorizedAccessException">The specified file already exists on the local filesystem.</exception>
        /// <exception cref="IOException">There was an error creating a specified file.</exception>
        public static RealtimeAddon New(string filename)
        {
            /*if (File.Exists(filename))
            {
                throw new UnauthorizedAccessException("The file already exists.");
            }*/

            if (Path.GetExtension(filename) != ".gma")
            {
                filename = Path.GetFileNameWithoutExtension(filename);
                filename += ".gma";
            }

            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                fs.SetLength(0);
            }
            catch (IOException)
            {
                throw;
            }

            // First, we create a bare empty addon with no data or files
            Writer.CreateEmpty(fs);
            fs.Flush();

            // And now when the file is made, the addon will be created from this default file
            fs.Close();
            fs.Dispose();

            RealtimeAddon realtime = RealtimeAddon.Load(filename, false);
            return realtime;
        }

        /// <summary>
        /// Private constructor setting up references and default values.
        /// </summary>
        private RealtimeAddon()
        {
            _Tags = new List<string>();
            _Title = "Default addon";
            WatchedFiles = new List<FileWatch>();
            Files = new List<ContentFile>();
            _Modified = false;
        }

        /// <summary>
        /// Initialises an addon using data from the specified FileStream
        /// </summary>
        /// <param name="stream">The FileStream where the Addon is stored. The stream must be readable and seekable.</param>
        /// <exception cref="ArgumentException">If there is a problem with the Stream.</exception>
        /// <exception cref="IOException">if an error occures in an I/O operation.</exception>
        /// <exception cref="ReaderException">If the addon has an invalid format.</exception>
        private RealtimeAddon(FileStream stream)
            : this()
        {
            AddonStream = stream;

            if (!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException("The specified stream cannot be read or seeked.");

            if (stream.Length == 0)
                throw new ReaderException("Attempted to read from empty buffer.");

            stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);

            // Ident
            if (String.Join(String.Empty, reader.ReadChars(RealtimeAddon.Ident.Length)) != RealtimeAddon.Ident)
                throw new ReaderException("Header mismatch.");

            FormatVersion = reader.ReadChar();
            if (FormatVersion > RealtimeAddon.Version)
                throw new ReaderException("Can't parse version " + Convert.ToString(FormatVersion) + " addons.");

            reader.ReadUInt64(); //SteamID = reader.ReadUInt64(); // SteamID (long)
            _Timestamp = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().
                AddSeconds((double)reader.ReadInt64()); // Timestamp (long)

            // Required content (not used at the moment, just read out)
            if (FormatVersion > 1)
            {
                string content = reader.ReadNullTerminatedString();

                while (content != String.Empty)
                    content = reader.ReadNullTerminatedString();
            }

            _Title = reader.ReadNullTerminatedString();
            _Description = reader.ReadNullTerminatedString();
            reader.ReadNullTerminatedString(); // This would be the author... currently not implemented
            reader.ReadInt32(); //Version = reader.ReadInt32(); // Addon version (unused)

            // File index
            IndexBlock = (ulong)reader.BaseStream.Position; // Save where the IndexBlock began
            int FileNumber = 1;
            int Offset = 0;

            while (reader.ReadInt32() != 0)
            {
                string path = reader.ReadNullTerminatedString();
                long size = reader.ReadInt64(); // long long (8)
                uint CRC = reader.ReadUInt32(); // unsigned long (4)
                long offset = Offset;

                IndexEntry entry = new IndexEntry(path, size, CRC, offset);
                ContentFile file = new ContentFile(this, entry);

                Files.Add(file);

                Offset += (int)entry.Size;
                ++FileNumber;
            }

            FileBlock = (ulong)reader.BaseStream.Position;

            // Try to parse the description
            _Type = String.Empty;
            _Description = Json.ParseDescription(_Description, ref _Type, ref _Tags);

            // Calculate the CRC for the read data
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] buffer_whole = new byte[reader.BaseStream.Length - 4];
            reader.BaseStream.Read(buffer_whole, 0, (int)reader.BaseStream.Length - 4);
            uint calculatedCRC = System.Cryptography.CRC32.ComputeChecksum(buffer_whole);
            
            // Read the written CRC
            reader.BaseStream.Seek(-4, SeekOrigin.End);
            uint addonCRC = reader.ReadUInt32();

            // QUESTION: Should we do something with CRC mismatch?
        }

        /// <summary>
        /// Reads the contents of the given file into the buffer.
        /// </summary>
        /// <param name="index">The index entry of the file to be read.</param>
        /// <param name="buffer">The buffer where the file should be read to.</param>
        /// <exception cref="IOException">Thrown if an I/O error occurs while reading the file.</exception>
        internal void ReadContents(IndexEntry index, byte[] buffer)
        {
            if ((long)FileBlock + index.Offset > AddonStream.Length || (long)FileBlock + index.Offset + index.Size > AddonStream.Length)
                throw new IOException("The file read attempt is reading behind the boundaries of the stream!");
            

            AddonStream.Seek((long)FileBlock + index.Offset, SeekOrigin.Begin);
            AddonStream.Read(buffer, 0, (int)index.Size);
        }

        /// <summary>
        /// Checks the given filename and converts it into a valid path (if possible)
        /// </summary>
        /// <param name="filename">The filename to check and transform</param>
        /// <returns>A valid path to store the file inside the addon</returns>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        public string GetValidPath(string filename)
        {
            // Prevent the need to read the contents of a file if it cannot be added.
            string path = Whitelist.GetMatchingString(filename);

            path = path.TrimStart('/').TrimEnd('/'); // Trim unneccessary slashes

            if (Files.Any(f => f.Path == path))
                throw new ArgumentException("A file with the same path is already added.");

            // Some file paths are disallowed from being added
            if (path == null || path == "")
                throw new WhitelistException("Path was empty.");
            else if (path.Contains(".."))
                throw new WhitelistException(path + ": contains upwards traversal.");
            if (path == "addon.json")
                // Never allow addon.json to be added
                throw new WhitelistException(path + ": is addon.json");
            if (!Whitelist.Check(path.ToLowerInvariant()))
                throw new WhitelistException(path + ": not allowed by whitelist.");

            return path;
        }

        /// <summary>
        /// Adds the specified file from the local filesystem to the encapsulated addon.
        /// </summary>
        /// <param name="filename">The path of the file to add.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        /// <exception cref="IOException">Thrown if a problem happens with opening the file.</exception>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        public void AddFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }

            string path = String.Empty;
            try
            {
                path = GetValidPath(filename);
            }
            catch (WhitelistException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                throw;
            }

            AddFile(path, fs);
        }

        /// <summary>
        /// Adds a content from a stream to the encapsulated addon using the specified internal path.
        /// The Stream must be readable.
        /// </summary>
        /// <param name="path">The path which the file should be added as.</param>
        /// <param name="content">The Stream containing the actual content.</param>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="ArgumentException">The Stream cannot be read</exception>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        public void AddFile(string path, Stream content)
        {
            if (!content.CanRead)
                throw new ArgumentException("The Stream cannot be read.", "content");

            try
            {
                path = GetValidPath(path);
            }
            catch (WhitelistException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            ContentFile file = new ContentFile(path, content);
            Files.Add(file);
            Files.Sort((f1, f2) => String.Compare(f1.Path, f2.Path, StringComparison.InvariantCulture));
            _Modified = true;
        }

        /// <summary>
        /// Adds an array of bytes to the encapsulated addon using the specified internal path.
        /// </summary>
        /// <param name="path">The path which the file should be added as.</param>
        /// <param name="content">The array of bytes containing the actual content.</param>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        public void AddFile(string path, byte[] content)
        {
            try
            {
                path = GetValidPath(path);
            }
            catch (WhitelistException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            ContentFile file = new ContentFile(path, content);
            Files.Add(file);
            Files.Sort((f1, f2) => String.Compare(f1.Path, f2.Path, StringComparison.InvariantCulture));
            _Modified = true;
        }

        /// <summary>
        /// Removes the specified file from the encapsulated addon.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        // TODO: Support some sort of an "undelete" mechanism.
        public void RemoveFile(string path)
        {
            List<ContentFile> fileToRemove = Files.Where(f => f.Path == path).ToList();

            if (fileToRemove.Count == 0)
                throw new FileNotFoundException("The file is not in the archive.");
            else
            {
                switch (fileToRemove.First().State)
                {
                    case ContentFile.FileState.Intact:
                        fileToRemove.First().MarkDeleted();
                        break;
                    case ContentFile.FileState.Modified:
                        fileToRemove.First().MarkDeleted();
                        fileToRemove.First().DisposeExternal();
                        break;
                    case ContentFile.FileState.Added:
                        // If an added file is deleted before being saved to the addon, it's like if it was never added at all.
                        Files.Remove(fileToRemove.First());
                        break;
                    //case ContentFile.FileState.Deleted:
                    default:
                        // Nothing happens if we delete an already deleted file.
                        break;
                }
            }
            
            _Modified = true;
        }

        /// <summary>
        /// Extracts a file from the encapsulated addon and saves it on the local filesystem.
        /// </summary>
        /// <param name="path">The path of the file within the addon to extract.</param>
        /// <param name="to">The path on the local filesystem where the file should be saved. If omitted,
        /// the file will be extracted to the application's current working directory.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist within the addon.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if a file already exists at the specified extract location.</exception>
        /// <exception cref="IOException">Thrown if there was a problem creating the extracted file.</exception>
        public void ExtractFile(string path, string to = null)
        {
            if (to == null || to == String.Empty)
                to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(path);
            else
            {
                string dir = Path.GetDirectoryName(to);

                if (dir == String.Empty)
                    to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(to);
            }

            ContentFile file = null;
            try
            {
                file = Files.Where(f => f.Path == path).First();
            }
            catch (InvalidOperationException)
            {
                throw new FileNotFoundException("The specified file " + path + " does not exist in the addon.");
            }

            if (File.Exists(to))
            {
                throw new UnauthorizedAccessException("A file at " + to + " already exists.");
            }

            FileStream extract;
            try
            {
                extract = new FileStream(to, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                throw;
            }

            byte[] buffer = file.Content;
            extract.Write(buffer, 0, (int)file.Size);
            extract.Flush();
            extract.Dispose();
        }

        /// <summary>
        /// Saves the specified file on the local filesystem and sets up a notifier FileWatch object
        /// to let the application keep track of the changes in the saved file.
        /// </summary>
        /// <param name="path">The path of the file within the addon to extract.</param>
        /// <param name="to">The path on the local filesystem where the file should be saved. If omitted,
        /// the file will be extracted to the application's current working directory.</param>
        /// <exception cref="ArgumentException">Thrown if an export for the current file already exists.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist within the addon.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if a file already exists at the specified extract location.</exception>
        /// <exception cref="IOException">Thrown if there was a problem creating the extracted file.</exception>
        public void ExportFile(string path, string to)
        {
            if (to == null || to == String.Empty)
                to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(path);
            else
            {
                string dir = Path.GetDirectoryName(to);

                if (dir == String.Empty)
                    to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(to);
            }

            if (WatchedFiles.Where(f => f.ContentPath == path).Count() != 0)
            {
                throw new ArgumentException("The specified file " + path + " is already exported.");
            }

            try
            {
                ExtractFile(path, to);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }

            FileSystemWatcher fsw = new FileSystemWatcher(Path.GetDirectoryName(to), Path.GetFileName(to));
            fsw.NotifyFilter = NotifyFilters.LastWrite;

            FileWatch watch = new FileWatch();
            watch.FilePath = to;
            watch.ContentPath = path;
            watch.Watcher = fsw;

            fsw.Changed += watch.OnChanged;
            fsw.EnableRaisingEvents = true;

            watch.FileChanged += fsw_Changed;

            WatchedFiles.Add(watch);
        }

        /// <summary>
        /// Fires if an exported file is changed on the local filesystem.
        /// </summary>
        private void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            FileWatch watch = null;
            try
            {
                watch = WatchedFiles.Where(f => f.FilePath == e.FullPath).First();
            }
            catch (InvalidOperationException)
            {
                // The watch for the file was removed earlier but the Watcher remained there.
                // This should not happen. But better safe than sorry.
                ((FileSystemWatcher)sender).Dispose();
            }

            if (Files.Where(f => f.Path == watch.ContentPath).Count() == 1)
            {
                watch.Modified = true;
            }
            else
            {
                // The file we exported and watched no longer exists in the addon.
                WatchedFiles.Remove(watch);
                ((FileSystemWatcher)sender).Dispose();
            }
        }

        /// <summary>
        /// Deletes the export of the specified file from the local filesystem and stops watching the changes.
        /// </summary>
        /// <param name="filename">The path of the file within the addon to be dropped.</param>
        /// <exception cref="FileNotFoundException">Thrown if there is no export for the file.</exception>
        /// <exception cref="IOException">Thrown if there was a problem deleting the file from the local filesystem.</exception>
        public void DropExport(string path)
        {
            FileWatch watch;
            try
            {
                watch = WatchedFiles.Where(f => f.ContentPath == path).First();
            }
            catch (InvalidOperationException)
            {
                throw new FileNotFoundException("There is no export for " + path);
            }

            watch.Watcher.Dispose();
            WatchedFiles.Remove(watch);

            try
            {
                File.Delete(watch.FilePath);
            }
            catch (IOException)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the encapsulated addon object's file entry with the changes of a previously exported file.
        /// </summary>
        /// <param name="path">The path of the file within the addon to pull the changes for.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified path does not correspond to an export.</exception>
        /// <exception cref="IOException">Thrown if there was a problem opening the exported file.</exception>
        public void Pull(string path)
        {
            FileWatch search = null;
            try
            {
                search = WatchedFiles.Where(f => f.ContentPath == path).First();
            }
            catch (InvalidOperationException)
            {
                throw new FileNotFoundException("There is no export for " + path);
            }


            if (search.Modified == false)
            {
                return;
            }

            ContentFile content = null;
            try
            {
                content = Files.Where(f => f.Path == search.ContentPath).First();
            }
            catch (InvalidOperationException)
            {
                // The file we exported and watched no longer exists in the addon.
                WatchedFiles.Remove(search);
                search.Watcher.Dispose();
            }

            FileStream fs;
            try
            {
                fs = new FileStream(search.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                throw;
            }

            byte[] contBytes = new byte[fs.Length];
            fs.Read(contBytes, 0, (int)fs.Length);

            content.Content = contBytes;

            fs.Close();
            fs.Dispose();

            search.Modified = false; // The exported file is no longer modified
            _Modified = true; // But the addon itself is
        }

        /// <summary>
        /// Gets the ContentFile entry for the specified path.
        /// </summary>
        /// <param name="path">The path of the file WITHIN the addon.</param>
        /// <returns>The ContentFile instance.</returns>
        /// <exception cref="FileNotFoundException">The specified file is not in the collection.</exception>
        public ContentFile GetFile(string path)
        {
            ContentFile file;

            try
            {
                file = Files.Where(e => e.Path == path).First();
            }
            catch (InvalidOperationException)
            {
                throw new FileNotFoundException("The file is not in the archive.");
            }

            return file;
        }

        /// <summary>
        /// Saves the changes of the addon to its file stream.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the Stream cannot be written.</exception>
        /// <exception cref="IOException">Happens if there is a problem with creating the addon into its stream.</exception>
        public void Save()
        {
            // Create a backup of the current addon
            string backupName = Path.GetFileNameWithoutExtension(ContentFile.GenerateExternalPath(AddonStream.Name)) + "_backup.gma";
            try
            {
                // It is needed to create a new, temporary file where we write the addon first
                // Without it, we would "undermount" the current file
                // And end up overwriting the addon from where AddonFile.Content gets the data we would write.
                using (FileStream backup = new FileStream(backupName,
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    backup.SetLength(0);
                    AddonStream.Seek(0, SeekOrigin.Begin);
                    AddonStream.CopyTo(backup);
                    backup.Flush();
                }
            }
            catch (IOException)
            {
                throw;
            }

            Writer.WriteResults results;

            try
            {
                results = Writer.Create(this, AddonStream);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }

            // Alter the neccessary metadata
            _Modified = false;
            IndexBlock = results.NewIndexBlock;
            FileBlock = results.NewFileBlock;

            // Patch the content files in the Addon's Files list
            int lastIdx = 0;
            foreach (KeyValuePair<uint, KeyValuePair<ContentFile, IndexEntry>> update in results.IndexUpdates)
            {
                int idxInList = Files.IndexOf(update.Value.Key, lastIdx);
                lastIdx = idxInList;

                update.Value.Key.DisposeExternal();
                ContentFile newCF = new ContentFile(this, update.Value.Value);
                Files[idxInList] = newCF; // The new one marks an Intact file
            }

            // Remove all Deleted files
            for (int i = 0; i < Files.Count; ++i)
            {
                ContentFile f = Files[i];

                if (Files[i].State == ContentFile.FileState.Deleted)
                {
                    Files.RemoveAt(i);
                    --i;
                }
            }

            // 
            
            try
            {
                File.Delete(backupName);
            }
            catch (Exception)
            {
                // Noop.
            }
        }
        
        /// <summary>
        /// Closes all connections of the current RealtimeAddon instance.
        /// This does NOT save the changes of the addon itself.
        /// </summary>
        public void Close()
        {
            foreach (FileWatch watch in WatchedFiles)
                watch.Watcher.Dispose();

            WatchedFiles.Clear();

            AddonStream.Close();
            AddonStream.Dispose();

            foreach (ContentFile f in Files)
                f.DisposeExternal();

            Files.Clear();

            IndexBlock = 0;
            FileBlock = 0;
            FormatVersion = (char)0;
            _Title = "(Closed addon!)";
        }
    }
}