using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace SharpGMad
{
    /// <summary>
    /// Represents a watcher declaration for an exported file.
    /// </summary>
    class FileWatch
    {
        /// <summary>
        /// Gets or sets the path of the file on the filesystem.
        /// </summary>
        public string FilePath;
        /// <summary>
        /// Gets or sets the path of the file in the loaded addon.
        /// </summary>
        public string ContentPath;
        /// <summary>
        /// Gets or sets whether the file is modified externally.
        /// </summary>
        public bool Modified;
        /// <summary>
        /// The integrated System.IO.FileSystemWatcher object.
        /// </summary>
        public FileSystemWatcher Watcher;
    }

    /// <summary>
    /// Encapsulates an Addon and provides the extended "realtime" functionality over it.
    /// </summary>
    class RealtimeAddon
    {
        /// <summary>
        /// The addon handled by the current RealtimeAddon instance.
        /// </summary>
        public Addon OpenAddon { get; private set; }
        /// <summary>
        /// The file handle of the current open addon.
        /// </summary>
        private FileStream AddonStream;
        /// <summary>
        /// The reader corresponding to the handling of this addon on the disk.
        /// </summary>
        private Reader AddonReader;
        /// <summary>
        /// Gets the file path of the addon on the local filesystem.
        /// </summary>
        public string AddonPath { get { return AddonStream.Name; } }
        /// <summary>
        /// Indicates whether the current addon is modified (the state in memory differs from the state of the filestream).
        /// </summary>
        private bool _modified;
        /// <summary>
        /// Gets whether the current addon is modified (the state in memory differs from the state of the filestream).
        /// It can also set the modified state to true.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if external assembly attempts to set value to false.</exception>
        public bool Modified
        {
            get
            {
                return _modified;
            }
            set
            {
                if (value)
                {
                    _modified = value;
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
        /// Contains the exported files.
        /// </summary>
        public List<FileWatch> WatchedFiles { get; private set; }


        /// <summary>
        /// Checks, if you can write to a specific file.
        /// </summary>
        /// <param name="filename">The path to the file on the local filesystem.</param>
        /// <returns>A Boolean, saying wether the file is writable.</returns>
        public static Boolean CanWrite(string filename)
        {
            //Check if the file exists
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }
            try
            {   
                //Open a new FileStream and test, if it's writeable
                using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) //Check if it's possible to write to the file
                {
                }
                return true; //file isn't locked
            }
            catch { }
            return false; //File is locked
        }

        /// <summary>
        /// Loads the specified addon from the local filesystem and encapsulates it within a realtime instance.
        /// </summary>
        /// <param name="filename">The path to the file on the local filesystem.</param>
        /// <param name="canWrite">The Boolean, saying wether you can write to the file.</param>
        /// <returns>A RealtimeAddon instance.</returns>
        /// <exception cref="FileNotFoundException">Happens if the specified file does not exist.</exception>
        /// <exception cref="IOException">Thrown if there is a problem opening the specified file.</exception>
        /// <exception cref="ReaderException">Thrown if the addon reader and parser encounters an error.</exception>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">There is a file prohibited from storing by the global whitelist.</exception>
        /// <exception cref="IgnoredException">There is a file prohibited from storing by the addon's ignore list.</exception>
        public static RealtimeAddon Load(string filename,Boolean canWrite)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }

            FileStream fs;
            try
            {
                if (canWrite)
                    fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                else
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                throw;
            }

            Reader r;
            try
            {
                r = new Reader(fs);
            }
            catch (IOException)
            {
                throw;
            }
            catch (ReaderException)
            {
                throw;
            }

            Addon addon;
            try
            {
                addon = new Addon(r);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (WhitelistException)
            {
                throw;
            }
            catch (IgnoredException)
            {
                throw;
            }

            RealtimeAddon realtime = new RealtimeAddon(addon, fs);
            realtime.AddonReader = r;
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
            if (File.Exists(filename))
            {
                throw new UnauthorizedAccessException("The file already exists.");
            }

            if (Path.GetExtension(filename) != "gma")
            {
                filename = Path.GetFileNameWithoutExtension(filename);
                filename += ".gma";
            }

            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                throw;
            }

            Addon addon = new Addon();

            RealtimeAddon realtime = new RealtimeAddon(addon, fs);
            return realtime;
        }

        /// <summary>
        /// Private constructor setting up references and default values.
        /// </summary>
        private RealtimeAddon()
        {
            WatchedFiles = new List<FileWatch>();
            _modified = false;
        }

        /// <summary>
        /// Creates the RealtimeAddon instance with the specified Addon to encapsulate and the FileStream pointing to the
        /// local filesystem. This method cannot be called externally.
        /// </summary>
        /// <param name="addon">The addon to encapsulate.</param>
        /// <param name="stream">The FileStream pointing to the GMA file on the local filesystem.</param>
        protected RealtimeAddon(Addon addon, FileStream stream)
            : this()
        {
            OpenAddon = addon;
            AddonStream = stream;
        }

        /// <summary>
        /// Adds the specified file from the local filesystem to the encapsulated addon.
        /// </summary>
        /// <param name="filename">The path of the file to add.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        /// <exception cref="IOException">Thrown if a problem happens with opening the file.</exception>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        /// <exception cref="IgnoredException">The file is prohibited from storing by the addon's ignore list.</exception>
        public void AddFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }

            // Prevent the need to read the contents of a file if it cannot be added.
            string path = Whitelist.GetMatchingString(filename);

            try
            {
                OpenAddon.CheckRestrictions(path);
            }
            catch (IgnoredException)
            {
                throw;
            }
            catch (WhitelistException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            byte[] bytes;

            try
            {
                bytes = File.ReadAllBytes(filename);
            }
            catch (IOException)
            {
                throw;
            }

            AddFile(Whitelist.GetMatchingString(filename), bytes);
        }

        /// <summary>
        /// Adds an array of bytes to the encapsulated addon using the specified internal path.
        /// </summary>
        /// <param name="path">The path which the file should be added as.</param>
        /// <param name="content">The array of bytes containing the actual content.</param>
        /// <exception cref="ArgumentException">Happens if a file with the same path is already added.</exception>
        /// <exception cref="WhitelistException">The file is prohibited from storing by the global whitelist.</exception>
        /// <exception cref="IgnoredException">The file is prohibited from storing by the addon's ignore list.</exception>
        public void AddFile(string path, byte[] content)
        {
            try
            {
                OpenAddon.AddFile(path, content);
            }
            catch (IgnoredException)
            {
                throw;
            }
            catch (WhitelistException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            _modified = true;
        }

        /// <summary>
        /// Removes the specified file from the encapsulated addon.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        public void RemoveFile(string path)
        {
            try
            {
                OpenAddon.RemoveFile(path);
            }
            catch (FileNotFoundException)
            {
                throw;
            }

            _modified = true;
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
            {
                to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(path);
            }
            else
            {
                string dir = Path.GetDirectoryName(to);

                if (dir == String.Empty)
                {
                    to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(to);
                }
            }

            ContentFile file = null;
            try
            {
                file = OpenAddon.Files.Where(f => f.Path == path).First();
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

            extract.Write(file.Content, 0, (int)file.Size);
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
            {
                to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(path);
            }
            else
            {
                string dir = Path.GetDirectoryName(to);

                if (dir == String.Empty)
                {
                    to = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(to);
                }
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
            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.EnableRaisingEvents = true;

            FileWatch watch = new FileWatch();
            watch.FilePath = to;
            watch.ContentPath = path;
            watch.Watcher = fsw;

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

            if (OpenAddon.Files.Where(f => f.Path == watch.ContentPath).Count() == 1)
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
                content = OpenAddon.Files.Where(f => f.Path == search.ContentPath).First();
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
            _modified = true; // But the addon itself is
        }

        /// <summary>
        /// Gets the ContentFile entry for the specified path.
        /// </summary>
        /// <param name="path">The path of the file WITHIN the addon.</param>
        /// <returns>The ContentFile instance.</returns>
        /// <exception cref="FileNotFoundException">The specified file is not in the collection.</exception>
        public ContentFile GetFile(string path)
        {
            try
            {
                return OpenAddon.GetFile(path);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the changes of the encapsulated addon to its file stream.
        /// </summary>
        /// <exception cref="IOException">Happens if there is a problem with creating the addon into its stream.</exception>
        public void Save()
        {
            OpenAddon.Sort();
            try
            {
                // It is needed to create a new, temporary file where we write the addon first
                // Without it, we would "undermount" the current file
                // And end up overwriting the addon from where ContentFile.Content gets the data we would write.
                using (FileStream newAddon = new FileStream(AddonStream.Name + "_create",
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    Writer.Create(OpenAddon, newAddon);

                    // Copy the contents to the real file
                    newAddon.Seek(0, SeekOrigin.Begin);
                    AddonStream.Seek(0, SeekOrigin.Begin);
                    AddonStream.SetLength(0);
                    newAddon.CopyTo(AddonStream);
                    AddonStream.Flush();
                }
            }
            catch (IOException)
            {
                throw;
            }

            // If there were no errors creating and copying the temporary file,
            // I assume it is safe to delete.
            File.Delete(AddonStream.Name + "_create");

            _modified = false;

            // Reload the content database of the open addon
            if (AddonReader == null)
            {
                try
                {
                    AddonReader = new Reader(AddonStream);
                }
                catch (IOException)
                {
                    throw;
                }
            }
            else
            {
                AddonReader.Reparse();
            }

            // Convert all files in the open addon to addon-backed content storages
            // So after save, the application knows the file is now in the addon.
            // This also updates the fileIDs in case of a file was reordered when Sort() happened.
            foreach (Reader.IndexEntry entry in AddonReader.Index)
            {
                OpenAddon.Files.Where(f => f.Path == entry.Path).First().SwitchToAddonInstance(AddonReader, entry);
            }
        }

        /// <summary>
        /// Closes all connections of the current RealtimeAddon instance.
        /// This does NOT save the changes of the encapsulated addon!
        /// </summary>
        public void Close()
        {
            foreach (FileWatch watch in WatchedFiles)
            {
                watch.Watcher.Dispose();
            }
            WatchedFiles.Clear();

            AddonStream.Close();
            AddonStream.Dispose();

            try
            {
                OpenAddon.Files.Clear();
            }
            catch (NullReferenceException)
            {
                // The addon was disposed earlier. Noop.
            }

            OpenAddon = null;
        }
    }
}