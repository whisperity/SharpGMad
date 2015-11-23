using System;
using System.IO;

namespace SharpGMad
{
    /// <summary>
    /// Represents a file belonging to an Addon.
    /// A ContentFile is not neccessarily physically inside the Addon's file!
    /// </summary>
    class ContentFile
    {
        /// <summary>
        /// Specifies the exact state of the ContentFile.
        /// </summary>
        internal enum FileState : byte
        {
            /// <summary>
            /// The file exists physically in the GMA and did NOT have its contents modified.
            /// </summary>
            Intact = 0x00,

            /// <summary>
            /// The file exists in the GMA but had its contents modified.
            /// </summary>
            Modified = 0x01,

            /// <summary>
            /// The file does NOT exist in the GMA but have been added.
            /// </summary>
            Added = 0x02,

            /// <summary>
            /// The file physically EXISTS in the GMA but has been marked for deletion.
            /// </summary>
            Deleted = 0x03
        }
        
        /// <summary>
        /// Gets the exact FileState of the ContentFile.
        /// </summary>
        public FileState State { get; private set; }

        /// <summary>
        /// The part of the ContentFile by which the contents physically within the addon can be get.
        /// </summary>
        private class IntactContent
        {
                /// <summary>
                /// The addon from where the file's data should be read.
                /// </summary>
                private RealtimeAddon Addon;

                /// <summary>
                /// The index entry which is used to pinpoint in the addon file where the content is.
                /// </summary>
                private IndexEntry Index;

                /// <summary>
                /// Initializes the Intact part of a ContentFile
                /// </summary>
                /// <param name="addon">The addon from which the file will be read from.</param>
                /// <param name="entry">The index metadata of the file to be read</param>
                public IntactContent(RealtimeAddon addon, IndexEntry entry)
                {
                    Addon = addon;
                    Index = entry;
                }

                public int Read(byte[] buffer)
                {
                    Addon.ReadContents(Index, buffer);

                    return (int)Index.Size;
                }

                public IndexEntry GetIndex() { return new IndexEntry(Index.Path, Index.Size, Index.CRC, Index.Offset); }
                public int Size { get { return (int)Index.Size; } }
        }

        /// <summary>
        /// The part of the ContentFile by which the contents in a temporary storage can be get or set.
        /// </summary>
        private class ExternalContent
        {
            /// <summary>
            /// The FileStream backend where the contents should be stored in.
            /// </summary>
            private FileStream Stream;

            /// <summary>
            /// Initializes the external part of the ContentFile for the given stream.
            /// </summary>
            /// <param name="stream">The file to store contents in.</param>
            public ExternalContent(FileStream stream)
            {
                Stream = stream;
            }

            /// <summary>
            /// Initializes the external part of the ContentFile for the given stream and writes the initial contents.
            /// </summary>
            /// <param name="file">The file to store contents in.</param>
            /// <param name="initial">The initial contents that should be written.</param>
            public ExternalContent(FileStream file, Stream initial)
            {
                Stream = file;

                file.Seek(0, SeekOrigin.Begin);
                initial.CopyTo(file);
                file.SetLength(file.Position);
            }

            public int Read(byte[] buffer)
            {
                Stream.Seek(0, SeekOrigin.Begin);
                Stream.Read(buffer, 0, (int)Stream.Length);

                return (int)Stream.Length;
            }

            public long Size { get { return Stream.Length; } }

            public void Write(byte[] buffer)
            {
                if (buffer == null || buffer.Length == 0)
                    Stream.SetLength(0);
                else
                {
                    Stream.SetLength(buffer.Length);
                    Stream.Seek(0, SeekOrigin.Begin);
                    Stream.Write(buffer, 0, buffer.Length);
                    Stream.Flush();
                }
            }

            /// <summary>
            /// Destroys the external file associated with the external part of the ContentFile.
            /// </summary>
            /// <exception cref="IOException">An I/O error occured.</exception>
            public void Destroy()
            {
                Stream.Dispose();
                File.Delete(Stream.Name);
            }
        }

        /// <summary>
        /// Generates an automatic temporary file name for a storage on the user's hard drive.
        /// </summary>
        /// <param name="filename">The filename for which the temporary file should be generated for.</param>
        /// <returns>The generated temporary path.</returns>
        /// <exception cref="IOException">A temporary file could not be generated.</exception>
        public static string GenerateExternalPath(string filename)
        {
            string tempfile = System.IO.Path.GetTempFileName();
            File.Delete(tempfile);

            string tempname = System.IO.Path.GetFileNameWithoutExtension(tempfile);
            tempname = System.IO.Path.GetTempPath() + tempname + "_sharpgmad_" + System.IO.Path.GetFileName(filename) + ".tmp";

            return tempname;
        }

        /// <summary>
        /// Accessing object for the contents physically in the GMA.
        /// </summary>
        private IntactContent Internal;

        /// <summary>
        /// Accessing object for contents in a temporary storage space.
        /// </summary>
        private ExternalContent External;
        
        /// <summary>
        /// The path of the file within the addon instance.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Retrieves the size of the ContentFile.
        /// </summary>
        public long Size
        {
            get
            {
                switch (State)
                {
                    case FileState.Intact:
                        return Internal.Size;
                    case FileState.Modified:
                    case FileState.Added:
                        return External.Size;
                    case FileState.Deleted: // Though the file is marked for deletion, it still exists in the internal storage.
                        return Internal.Size;
                    default:
                        throw new InvalidOperationException("Unknwon file state.");
                }
            }
        }

        /// <summary>
        /// Initializes a new ContentFile for a given logical file already existing in an addon.
        /// </summary>
        /// <param name="addon">The addon.</param>
        /// <param name="index">The index of the file to use.</param>
        public ContentFile(RealtimeAddon addon, IndexEntry index)
        {
            State = FileState.Intact;
            Path = index.Path;

            Internal = new IntactContent(addon, index);
            External = null;
        }

        /// <summary>
        /// Initializes a new ContentFile for a given logical file and byte content.
        /// </summary>
        /// <param name="filename">The filename of the logical file.</param>
        /// <param name="content">The (initial) contents of the logical file.</param>
        public ContentFile(string filename, byte[] content)
        {
            State = FileState.Added;
            Path = filename;

            Internal = null;
            
            FileStream backend = new FileStream(GenerateExternalPath(filename), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            External = new ExternalContent(backend);
            External.Write(content);
        }

        /// <summary>
        /// Initializes a new ContentFile for a given logical file and Stream contents.
        /// </summary>
        /// <param name="filename">The filename of the logical file.</param>
        /// <param name="content">The (initial) contents of the logical file contained within a Stream.</param>
        /// <exception cref="ArgumentException">Thrown if the stream cannot be read.</exception>
        public ContentFile(string filename, Stream content)
        {
            if (!content.CanRead)
                throw new ArgumentException("The Stream cannot be read.", "content");

            State = FileState.Added;
            Path = filename;

            Internal = null;
            FileStream backend = new FileStream(GenerateExternalPath(filename), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            External = new ExternalContent(backend, content);
        }

        /// <summary>
        /// Retrieve or modify the contents of the file.
        /// Modyfing a file will make use of switching the file to a different storage medium.
        /// </summary>
        public byte[] Content
        {
            get
            {
                byte[] retValue = new byte[Size];

                switch (State)
                {
                    case FileState.Intact:
                        Internal.Read(retValue);
                        break;
                    case FileState.Modified:
                    case FileState.Added:
                        External.Read(retValue);
                        break;
                    case FileState.Deleted:
                        throw new AccessViolationException("Should not access contents of a deleted file.");
                    default:
                        throw new InvalidOperationException("Unknown file state.");
                }

                return retValue;
            }
            set
            {
                if (State == FileState.Deleted)
                    throw new InvalidOperationException("Cannot modify contents of a deleted file.");
                else if (State == FileState.Intact)
                {
                    // If an Intact file has its contents modified, an external copy must be created
                    FileStream externalStorage = new FileStream(GenerateExternalPath(Path), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                    External = new ExternalContent(externalStorage);

                    State = FileState.Modified; // And we consider the file modified from now on.
                }

                External.Write(value);
            }
        }

        /// <summary>
        /// Mark the file for deletion.
        /// </summary>
        public void MarkDeleted()
        {
            State = FileState.Deleted;
        }

        /// <summary>
        /// Gets a CRC32 checksum for the content.
        /// </summary>
        public uint CRC
        {
            get
            {
                switch (State)
                {
                    case FileState.Intact:
                        return Internal.GetIndex().CRC;
                    case FileState.Modified:
                    case FileState.Added:
                        return System.Cryptography.CRC32.ComputeChecksum(Content);
                    case FileState.Deleted:
                        throw new AccessViolationException("Should not access contents of a deleted file.");
                    default:
                        throw new InvalidOperationException("Unknown file state.");
                }
            }
        }

        /// <summary>
        /// Disposes the externally saved content backend for the current file.
        /// </summary>
        public void DisposeExternal()
        {
            try
            {
                if (State == FileState.Added || State == FileState.Modified)
                    External.Destroy();
            }
            catch (Exception)
            {
                // We couldn't delete the file. Noop, we just leave it there.
            }
        }

        /// <summary>
        /// Cleans up the temporary folder from possible stale externally saved content files.
        /// </summary>
        public static void DisposeExternals()
        {
            foreach (string file in Directory.GetFiles(System.IO.Path.GetTempPath(),
                "tmp*_sharpgmad_*.tmp*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    // Noop. It is only a temporary file, it shouldn't be that bad if we don't clean it up.
                }
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(State);
            sb.Append(" ");
            sb.Append(Path);
            sb.Append(" ");
            if (Internal != null)
                sb.Append("Internal - " + Internal.Size + " bytes beginning at offset " + Internal.GetIndex().Offset);
            if (Internal != null && External != null)
                sb.Append("  ");
            if (External != null)
                sb.Append("External - " + External.Size + " bytes");

            return sb.ToString();
        }

        /// <summary>
        /// Automatically disposes (if neccessary), the external temporary files when the object is finalised.
        /// </summary>
        ~ContentFile()
        {
            DisposeExternal();
        }
    }
}
