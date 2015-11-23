using System.IO;

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

        /// <summary>
        /// The event to fire when the assigned Watcher reports a file change.
        /// </summary>
        public event FileSystemEventHandler FileChanged;

        /// <summary>
        /// Fires all associated FileChanged delegates.
        /// </summary>
        public void OnChanged(object sender, FileSystemEventArgs e)
        {
            foreach (FileSystemEventHandler handler in FileChanged.GetInvocationList())
                handler.Invoke(sender, e);
        }

        public override string ToString()
        {
            return ContentPath + " <==> " + FilePath + (Modified ? " [Modified]" : System.String.Empty);
        }
    }
}
