using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace Addon
{
    class Reader
    {
        protected MemoryStream m_buffer;
        protected char m_fmtversion;
        protected string m_name;
        protected string m_author;
        protected string m_desc;
        protected string m_type;
        protected List<Addon.Format.FileEntry> m_index;
        protected ulong m_fileblock;

        List<string> m_tags;

        private string ReadStringNULDelimiter(BinaryReader br)
        {
            List<byte> bytes = new List<byte>();
            byte read;

            while ((read = br.ReadByte()) != 0x00)
                bytes.Add(read);

            return (bytes.Count > 0 ? Encoding.UTF8.GetString(bytes.ToArray()) : "");
        }

        public Reader()
        {
            m_buffer = new MemoryStream();
            m_index = new List<Addon.Format.FileEntry>();

            Clear();
        }

        //
        // Load an addon (call Parse after this succeeds)
        //
        public bool ReadFromFile(string strName)
        {
            // m_buffer.Clear()
            m_buffer.Seek(0, SeekOrigin.Begin);
            m_buffer.SetLength(0);
            
            using (FileStream gmafs = new FileStream(strName, FileMode.Open, FileAccess.Read))
            {
                while (gmafs.Position < gmafs.Length)
                {
                    m_buffer.WriteByte((byte)gmafs.ReadByte());
                }
            }

            return true;
        }

        //
        // Parse the addon into this class
        //
        public bool Parse()
        {
            m_buffer.Seek(0, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(m_buffer);

            // Ident
            if (br.ReadChar() != 'G' ||
                br.ReadChar() != 'M' ||
                br.ReadChar() != 'A' ||
                br.ReadChar() != 'D')
            {
                
                return false;
            }

            m_fmtversion = br.ReadChar();

            if (m_fmtversion > Addon.Format.Version)
                return false;

            br.ReadInt64(); // steamid (long)
            br.ReadInt64(); // timestamp (long)

            //
            // Required content (not used at the moment, just read out)
            //
            if (m_fmtversion > 1)
            {
                string strContent = ReadStringNULDelimiter(br);

                while (strContent != String.Empty)
                {
                    strContent = ReadStringNULDelimiter(br);
                }
            }

            m_name = ReadStringNULDelimiter(br);
            m_desc = ReadStringNULDelimiter(br);
            m_author = ReadStringNULDelimiter(br);

            //
            // Addon version - unused
            //
            br.ReadInt32();

            //
            // File index
            //
            int iFileNumber = 1;
            int iOffset = 0;

            while (br.ReadUInt32() != 0)
            {
                Addon.Format.FileEntry entry = new Addon.Format.FileEntry();
                entry.strName = ReadStringNULDelimiter(br);
                entry.iSize = br.ReadInt64(); // long long
                entry.iCRC = br.ReadUInt32(); // unsigned long
                entry.iOffset = iOffset;
                entry.iFileNumber = (uint)iFileNumber;

                m_index.Add(entry);

                iOffset += (int)entry.iSize;
                iFileNumber++;
            }

            m_fileblock = (ulong)br.BaseStream.Position;
            //
            // Try to parse the description
            //
            using (MemoryStream desc_stream = new MemoryStream(Encoding.UTF8.GetBytes(m_desc)))
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DescriptionJSON));
                try
                {
                    DescriptionJSON description = (DescriptionJSON)jsonFormatter.ReadObject(desc_stream);

                    m_desc = description.Description;
                    m_type = description.Type;
                    m_tags = description.Tags;
                }
                catch (SerializationException)
                {
                    // Noop. The description will stay because it was plaintext, not JSON.
                    m_type = String.Empty;
                    m_tags = new List<string>();
                }
            }

            return true;
        }

        [DataContract]
        internal class DescriptionJSON
        {
            [DataMember(Name="description")]
            public string Description;

            [DataMember(Name = "type")]
            public string Type;

            [DataMember(Name = "tags")]
            public List<string> Tags;
        }

        //
        // Return the FileEntry for a FileID
        //
        public bool GetFile(uint iFileID, out Addon.Format.FileEntry outfile)
        {
            outfile = new Addon.Format.FileEntry();

            foreach (Addon.Format.FileEntry file in m_index)
            {
                if (file.iFileNumber == iFileID)
                {
                    outfile = file;
                    return true;
                }
            }
            return false;
        }

        //
        // Read a fileid from the addon into the buffer
        //
        public bool ReadFile(uint iFileID, MemoryStream buffer)
        {
            Addon.Format.FileEntry file;
            if (!GetFile(iFileID, out file)) return false;

            byte[] read_buffer = new byte[file.iSize];
            m_buffer.Seek((long)m_fileblock + (long)file.iOffset, SeekOrigin.Begin);
            m_buffer.Read(read_buffer, 0, (int)file.iSize);

            buffer.Write(read_buffer, 0, read_buffer.Length);
            return true;
        }

        public void Clear()
        {
            // m_buffer.Clear()
            m_buffer.Seek(0, SeekOrigin.Begin);
            m_buffer.SetLength(0);

            m_fmtversion = (char)0;
            m_name = String.Empty;
            m_author = String.Empty;
            m_desc = String.Empty;
            m_index.Clear();
            m_type = String.Empty;
            m_fileblock = 0;

            if ( m_tags != null )
                m_tags.Clear();
        }

        // Getters... please remove later.
        public List<Addon.Format.FileEntry> GetList() { return m_index; }
        public uint GetFormatVersion() { return m_fmtversion; }
        public MemoryStream GetBuffer() { return m_buffer; }
        public string Title() { return m_name; }
        public string Description() { return m_desc; }
        public string Author() { return m_author; }
        public string Type() { return m_type; }
        public List<string> Tags() { return m_tags; }
    }
}