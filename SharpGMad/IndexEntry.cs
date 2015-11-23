namespace SharpGMad
{
    /// <summary>
    /// Contains indexing information about a file entry in a GMA.
    /// </summary>
    class IndexEntry
    {
        /// <summary>
        /// Gets the internal path of the file.
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// Gets the size of the contents.
        /// </summary>
        public long Size { get; private set; }
        /// <summary>
        /// Gets a CRC32 checksum for the content.
        /// </summary>
        public uint CRC { get; private set; }
        /// <summary>
        /// The offset (relative to the addon's FileBlock) where the contents begin.
        /// </summary>
        public long Offset { get; private set; }

        /// <summary>
        /// Initializes a new IndexEntry with the given parameters.
        /// </summary>
        /// <param name="path">The internal path of the file.</param>
        /// <param name="size">The size of the contents.</param>
        /// <param name="crc">A CRC32 hash of the contents.</param>
        /// <param name="offset">The offset (relative to FileBlock) where the contents actually begin.</param>
        public IndexEntry(string path, long size, uint crc, long offset)
        {
            Path = path;
            Size = size;
            CRC = crc;
            Offset = offset;
        }

        public override string ToString()
        {
            return Path + " (" + Size + " bytes, " + System.Int32Extensions.HumanReadableSize((int)Size) +" beginning at " + Offset + ") [" + CRC + "]";
        }
    }
}
