﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

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
        protected ReaderException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// Provides methods for reading a compiled GMA file.
    /// </summary>
    class Reader
    {
        /// <summary>
        /// Represents a file's entry in the GMA index.
        /// </summary>
        public struct IndexEntry
        {
            /// <summary>
            /// The path of the file.
            /// </summary>
            public string Path;
            /// <summary>
            /// The size (in bytes) of the file.
            /// </summary>
            public long Size;
            /// <summary>
            /// The CRC checksum of file contents.
            /// </summary>
            public ulong CRC;
            /// <summary>
            /// The index of the file.
            /// </summary>
            public uint FileNumber;
            /// <summary>
            /// The offset (in bytes) where the file content is stored in the GMA.
            /// </summary>
            public long Offset;
        }

        /// <summary>
        /// The internal buffer where the addon is loaded.
        /// </summary>
        private MemoryStream Buffer;
        /// <summary>
        /// The byte representing the version character.
        /// </summary>
        private char FormatVersion;
        /// <summary>
        /// Gets the name of the addon.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the author of the addon. (Currently unused, will always return "Author Name.")
        /// </summary>
        public string Author { get; private set; }
        /// <summary>
        /// Gets the description of the addon.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Gets the type of the addon.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Represents the index area of the addon.
        /// </summary>
        private List<IndexEntry> _Index;
        /// <summary>
        /// Gets a list of files.
        /// </summary>
        public List<IndexEntry> Index { get { return new List<IndexEntry>(_Index); } }
        /// <summary>
        /// Represents the offset where the file content storage begins.
        /// </summary>
        private ulong Fileblock;
        /// <summary>
        /// Contains a list of strings, the tags of the read addon.
        /// </summary>
        private List<string> _Tags;
        /// <summary>
        /// Gets a list of addon tags.
        /// </summary>
        public List<string> Tags { get { return new List<string>(_Tags); } }

        /// <summary>
        /// Private constructor to set up object references.
        /// </summary>
        private Reader()
        {
            Buffer = new MemoryStream();
            _Index = new List<IndexEntry>();
            _Tags = new List<string>();
        }

        /// <summary>
        /// Reads and parses the specified addon file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <exception cref="System.IO.IOException">Any sort of error regarding access to the specified file.</exception>
        /// <exception cref="ReaderException">Errors parsing the file</exception>
        public Reader(string path)
            : this()
        {
            try
            {
                using (FileStream gmaFileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    gmaFileStream.CopyTo(Buffer);
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }

            try
            {
                Parse();
            }
            catch (ReaderException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reads and parses the specified addon file.
        /// </summary>
        /// <param name="stream">The file stream representing the addon file.</param>
        /// <exception cref="System.IO.IOException">Any sort of error regarding reading from the provided stream.</exception>
        /// <exception cref="ReaderException">Errors parsing the file</exception>
        public Reader(FileStream stream)
            : this()
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(Buffer);
            }
            catch (IOException ex)
            {
                throw ex;
            }

            try
            {
                Parse();
            }
            catch (ReaderException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Parses the read addon stream into the instance properties.
        /// </summary>
        /// <exception cref="ReaderException">Parsing errors.</exception>
        private void Parse()
        {
            if (Buffer.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Attempted to read from empty buffer.");
                Console.ResetColor();
                throw new ReaderException("Attempted to read from empty buffer.");
            }

            Buffer.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(Buffer);

            // Ident
            if (String.Join(String.Empty, reader.ReadChars(Addon.Ident.Length)) != Addon.Ident)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Header mismatch.");
                Console.ResetColor();
                throw new ReaderException("Header mismatch.");
            }

            FormatVersion = reader.ReadChar();
            if (FormatVersion > Addon.Version)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Can't parse version " + Convert.ToString(FormatVersion) + " addons.");
                Console.ResetColor();
                throw new ReaderException("Can't parse version " + Convert.ToString(FormatVersion) + " addons.");
            }

            reader.ReadInt64(); // SteamID (long)
            reader.ReadInt64(); // Timestamp (long)

            // Required content (not used at the moment, just read out)
            if (FormatVersion > 1)
            {
                string content = reader.ReadNullTerminatedString();

                while (content != String.Empty)
                    content = reader.ReadNullTerminatedString();
            }

            Name = reader.ReadNullTerminatedString();
            Description = reader.ReadNullTerminatedString();
            Author = reader.ReadNullTerminatedString();
            reader.ReadInt32(); // Addon version (unused)

            // File index
            int FileNumber = 1;
            int Offset = 0;

            while (reader.ReadInt32() != 0)
            {
                IndexEntry entry = new IndexEntry();
                entry.Path = reader.ReadNullTerminatedString();
                entry.Size = reader.ReadInt64(); // long long
                entry.CRC = reader.ReadUInt32(); // unsigned long
                entry.Offset = Offset;
                entry.FileNumber = (uint)FileNumber;

                _Index.Add(entry);

                Offset += (int)entry.Size;
                FileNumber++;
            }

            Fileblock = (ulong)reader.BaseStream.Position;

            // Try to parse the description
            using (MemoryStream descStream = new MemoryStream(Encoding.ASCII.GetBytes(Description)))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(DescriptionJSON));
                try
                {
                    DescriptionJSON dJSON = (DescriptionJSON)jsonSerializer.ReadObject(descStream);

                    Description = dJSON.Description;
                    Type = dJSON.Type;
                    _Tags = new List<string>(dJSON.Tags);
                }
                catch (SerializationException)
                {
                    // The description is a plaintext in the file.
                    Type = String.Empty;
                    _Tags = new List<string>();
                }
            }
        }

        /// <summary>
        /// Gets the index entry for the specified file.
        /// </summary>
        /// <param name="fileID">The index of the file.</param>
        /// <param name="entry">The IndexEntry object to be filled with data.</param>
        /// <returns>True if the entry was successfully found, false otherwise.</returns>
        public bool GetEntry(uint fileID, out IndexEntry entry)
        {
            if (Index.Where(file => file.FileNumber == fileID).Count() == 0)
            {
                entry = new IndexEntry();
                return false;
            }
            else
            {
                entry = Index.Where(file => file.FileNumber == fileID).First();
                return true;
            }
        }

        /// <summary>
        /// Gets the specified file contents from the addon.
        /// </summary>
        /// <param name="fileID">The index of the file.</param>
        /// <param name="buffer">The stream the contents should be written to.</param>
        /// <returns>True if the file was successfully read, false otherwise.</returns>
        public bool GetFile(uint fileID, MemoryStream buffer)
        {
            IndexEntry entry;
            if (!GetEntry(fileID, out entry)) return false;

            byte[] read_buffer = new byte[entry.Size];
            Buffer.Seek((long)Fileblock + (long)entry.Offset, SeekOrigin.Begin);
            Buffer.Read(read_buffer, 0, (int)entry.Size);

            buffer.Write(read_buffer, 0, read_buffer.Length);
            return true;
        }
    }
}