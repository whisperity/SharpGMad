using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Addon;

namespace SharpGMad
{
    class Reader
    {
        private MemoryStream Buffer;
        private char FormatVersion;
        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public string Type { get; private set; }
        private List<Addon.Format.FileEntry> _Index;
        public List<Addon.Format.FileEntry> Index
        {
            get
            {
                return new List<Addon.Format.FileEntry>(_Index);
            }
        }
        private ulong Fileblock;
        private List<string> _Tags;
        public List<string> Tags
        {
            get
            {
                return new List<string>(_Tags);
            }
        }

        private Reader()
        {
            Buffer = new MemoryStream();
            _Index = new List<Addon.Format.FileEntry>();
            _Tags = new List<string>();
        }

        public Reader(string path) : this()
        {
            try
            {
                using (FileStream gmaFileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    gmaFileStream.CopyTo(Buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load file. An exception happened.");
                Console.WriteLine(ex.Message);
                throw new Exception(String.Empty, ex);
            }

            Parse();
        }

        public Reader(FileStream stream) : this()
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(Buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to load file from stream. An exception happened.");
                Console.WriteLine(ex.Message);
                throw new Exception(String.Empty, ex);
            }

            Parse();
        }

        private void Parse()
        {
            if (Buffer.Length == 0)
                throw new Exception("Attempted to read from empty buffer.");

            Buffer.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(Buffer);

            // Ident
            if ( String.Join(String.Empty, reader.ReadChars(Addon.Format.Ident.Length)) != Addon.Format.Ident)
                throw new Exception("Header mismatch.");

            FormatVersion = reader.ReadChar();
            if (FormatVersion > Addon.Format.Version)
                throw new Exception("Can't parse version " + Convert.ToString(FormatVersion) + " addons.");

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
                Addon.Format.FileEntry entry = new Addon.Format.FileEntry();
                entry.strName = reader.ReadNullTerminatedString();
                entry.iSize = reader.ReadInt64(); // long long
                entry.iCRC = reader.ReadUInt32(); // unsigned long
                entry.iOffset = Offset;

                _Index.Add(entry);

                Offset += (int)entry.iSize;
                FileNumber++;
            }

            Fileblock = (ulong)reader.BaseStream.Position;

            // Try to parse the description
            using (MemoryStream descStream = new MemoryStream(Encoding.ASCII.GetBytes(Description)))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Addon.DescriptionJSON));
                try
                {
                    Addon.DescriptionJSON dJSON = (Addon.DescriptionJSON)jsonSerializer.ReadObject(descStream);

                    Description = dJSON.Description;
                    Type = dJSON.Type;
                    _Tags = dJSON.Tags;
                }
                catch (SerializationException)
                {
                    // The description is a plaintext in the file.
                    Type = String.Empty;
                    _Tags = new List<string>();
                }
            }
        }

        public bool GetEntry(uint fileID, out Addon.Format.FileEntry entry)
        {
            if (Index.Where(file => file.iFileNumber == fileID).Count() == 0)
            {
                entry = new Addon.Format.FileEntry();
                return false;
            }
            else
            {
                entry = Index.Where(file => file.iFileNumber == fileID).First();
                return true;
            }
        }

        public bool GetFile(uint fileID, MemoryStream buffer)
        {
            Addon.Format.FileEntry entry;
            if (!GetEntry(fileID, out entry)) return false;

            byte[] read_buffer = new byte[entry.iSize];
            Buffer.Seek((long)Fileblock + (long)entry.iOffset, SeekOrigin.Begin);
            Buffer.Read(read_buffer, 0, (int)entry.iSize);

            buffer.Write(read_buffer, 0, read_buffer.Length);
            return true;
        }
    }
}