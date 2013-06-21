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
    
    class RealtimeAddon
    {
        public Addon OpenAddon { get; private set; }
        private FileStream AddonStream;
        public string AddonPath { get { return AddonStream.Name;}}
        public bool Modified;
        public bool Pullable;
        public List<FileWatch> WatchedFiles { get; private set; }

        static public RealtimeAddon Load(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }

            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException e)
            {
                throw e;
            }

            Reader r;
            try
            {
                r = new Reader(fs);
            }
            catch (IOException e)
            {
                throw e;
            }
            catch (ReaderException e)
            {
                throw e;
            }

            Addon addon;
            try
            {
                addon = new Addon(r);
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (WhitelistException e)
            {
                throw e;
            }
            catch (IgnoredException e)
            {
                throw e;
            }

            RealtimeAddon realtime = new RealtimeAddon(addon, fs);
            return realtime;
        }

        static public RealtimeAddon New(string filename)
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
            catch (IOException e)
            {
                throw e;
            }

            Addon addon = new Addon();

            RealtimeAddon realtime = new RealtimeAddon(addon, fs);
            return realtime;
        }

        private RealtimeAddon()
        {
            WatchedFiles = new List<FileWatch>();
            Modified = false;
            Pullable = false;
        }

        protected RealtimeAddon(Addon addon, FileStream stream)
            : this()
        {
            OpenAddon = addon;
            AddonStream = stream;
        }

        public void AddFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file " + filename + " does not exist.");
            }

            byte[] bytes;

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, (int)fs.Length);
                }
            }
            catch(IOException e)
            {
                throw e;
            }

            AddFile(Whitelist.GetMatchingString(filename), bytes);
        }

        public void AddFile(string path, byte[] content)
        {
            try
            {
                OpenAddon.AddFile(path, content);
            }
            catch (IgnoredException e)
            {
                throw e;
            }
            catch (WhitelistException e)
            {
                throw e;
            }
            catch (ArgumentException e)
            {
                throw e;
            }

            Modified = true;
        }

        public void RemoveFile(string path)
        {
            try
            {
                OpenAddon.RemoveFile(path);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }

            Modified = true;
        }

        public void ExtractFile(string path, string to)
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
            catch(InvalidOperationException)
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
            catch (IOException e)
            {
                throw e;
            }

            extract.Write(file.Content, 0, (int)file.Size);
            extract.Flush();
            extract.Dispose();
        }

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

            try
            {
                ExtractFile(path, to);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
            catch (UnauthorizedAccessException e)
            {
                throw e;
            }
            catch (IOException e)
            {
                throw e;
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
                Pullable = true;
            }
            else
            {
                // The file we exported and watched no longer exists in the addon.
                WatchedFiles.Remove(watch);
                ((FileSystemWatcher)sender).Dispose();
            }
        }

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
            catch (IOException e)
            {
                throw e;
            }
        }

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
            catch (IOException e)
            {
                throw e;
            }

            byte[] contBytes = new byte[fs.Length];
            fs.Read(contBytes, 0, (int)fs.Length);

            content.Content = contBytes;

            fs.Close();
            fs.Dispose();

            search.Modified = false; // The exported file is no longer modified
            Modified = true; // But the addon itself is
        }

        public void Close()
        {
            foreach (FileWatch watch in WatchedFiles)
            {
                watch.Watcher.Dispose();
            }
            WatchedFiles.Clear();

            AddonStream.Close();
            AddonStream.Dispose();

            OpenAddon.Files.Clear();
            OpenAddon = null;
        }

        public void Save()
        {
            OpenAddon.Sort();
            Writer.Create(OpenAddon, AddonStream);

            Modified = false;
        }
    }
}