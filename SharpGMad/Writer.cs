using System;
using System.Collections.Generic;
using System.IO;

namespace SharpGMad
{
    /// <summary>
    /// Provides ways for compiling addons.
    /// </summary>
    static class Writer
    {
        /// <summary>
        /// Creates a bare, but valid Addon into the specified stream.
        /// </summary>
        /// <param name="stream">The stream which the result should be written to. It must be readable, seekable and writable.</param>
        /// <exception cref="ArgumentException">Thrown if the specified Stream cannot be read, seeked or written.</exception>
        /// <exception cref="IOExecption">Happens if an IO error happens while writing the stream.</exception>
        internal static void CreateEmpty(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek || !stream.CanWrite)
                throw new ArgumentException("The output stream cannot be read, seeked or written.");

            BinaryWriter writer = new BinaryWriter(stream);
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.BaseStream.SetLength(0);

            // Header (5)
            writer.Write(RealtimeAddon.Ident.ToCharArray()); // Ident (4)
            writer.Write((char)RealtimeAddon.Version); // Version (1)
            // SteamID (8) [unused]
            writer.Write((ulong)0);
            // TimeStamp (8)
            writer.Write((ulong)
                (((TimeSpan)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime())).TotalSeconds));
            // Required content (a list of strings)
            writer.Write((char)0); // signifies nothing
            // Addon Name (n)
            writer.WriteNullTerminatedString("Empty title");
            // Addon Description (n)
            writer.WriteNullTerminatedString(Json.BuildDescription("Empty Description", Tags.Type[0], new List<string>()));
            // Addon Author (n) [unused]
            writer.WriteNullTerminatedString("Author Name");
            // Addon version (4) [unused]
            writer.Write((int)1);

            // File list
            uint fileNum = 0;

            // Zero to signify the end of files
            writer.Write(fileNum);

            // The files
            // Nothing.

            // CRC what we've written (to verify that the download isn't shitted) (4)
            writer.Seek(0, SeekOrigin.Begin);
            byte[] buffer_whole = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(buffer_whole, 0, (int)writer.BaseStream.Length);
            uint addonCRC = System.Cryptography.CRC32.ComputeChecksum(buffer_whole);
            writer.Write(addonCRC);
            writer.Flush();

            writer.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Contains information about the results of a Create operation.
        /// </summary>
        public class WriteResults
        {
            /// <summary>
            /// The offset where the index area of the addon begins after the write.
            /// </summary>
            public ulong NewIndexBlock;
            /// <summary>
            /// The offset where the file block of the addon begins after the write.
            /// </summary>
            public ulong NewFileBlock;
            /// <summary>
            /// The list of updated files along with their new index entries.
            /// </summary>
            public SortedList<uint, KeyValuePair<ContentFile, IndexEntry>> IndexUpdates = new SortedList<uint, KeyValuePair<ContentFile, IndexEntry>>();
        }
        
        /// <summary>
        /// Updates the given Stream with the contents of the addon.
        /// </summary>
        /// <param name="rAddon">The Realtime Addon to write into the Stream.</param>
        /// <param name="stream">The Stream where the output should be written to. It must be readable, seekable and writable.
        /// The Stream must be the same stream where the original </param>
        /// <exception cref="ArgumentException">Thrown if the output stream cannot be accessed properly.</exception>
        /// <exception cref="IOException">Thrown if an I/O error happens while writing the output.</exception>
        public static WriteResults Create(RealtimeAddon rAddon, Stream stream)
        {
            if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek)
                throw new ArgumentException("The stream to write to cannot be read, seeked or written.", "stream");
            
            BinaryReader br = new BinaryReader(stream);
            BinaryWriter bw = new BinaryWriter(stream);

            stream.Seek(0, SeekOrigin.Begin);
            if (stream.Length == 0)
                throw new ArgumentException("Cannot create addon into an empty stream! Please use CreateEmpty() to create a default structure.");
            else
            {
                WriteResults Wresults = new WriteResults();

                // We are overwriting an existing addon.
                long indexBlock = (long)rAddon.IndexBlock;
                long fileBlock = (long)rAddon.FileBlock;
                
                // --- Metadata ---
                // We need to overwrite and align the metadata of the addon
                
                using (MemoryStream metadataBlock = new MemoryStream())
                using (BinaryWriter metaWriter = new BinaryWriter(metadataBlock))
                {
                    // Header (5)
                    metaWriter.Write(RealtimeAddon.Ident.ToCharArray()); // Ident (4)
                    metaWriter.Write((char)RealtimeAddon.Version); // Version (1)
                    // SteamID (8) [unused]
                    metaWriter.Write((ulong)0);
                    // TimeStamp (8)
                    metaWriter.Write((ulong)
                        (((TimeSpan)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime())).TotalSeconds));
                    // Required content (a list of strings)
                    metaWriter.Write((char)0); // signifies nothing
                    // Addon Name (n)
                    metaWriter.WriteNullTerminatedString(rAddon.Title);
                    // Addon Description (n)
                    metaWriter.WriteNullTerminatedString(Json.BuildDescription(rAddon.Description, rAddon.Type, new List<string>(rAddon.Tags)));
                    // Addon Author (n) [unused]
                    metaWriter.WriteNullTerminatedString("Author Name");
                    // Addon version (4) [unused]
                    metaWriter.Write((int)1);


                    if ((ulong)metadataBlock.Length != rAddon.IndexBlock)
                    {
                        // The metadata will be longer or shorter -> we need to make or take space
                        long difference = metadataBlock.Length - ((long)rAddon.IndexBlock);
                        stream.MoveEndPart((long)rAddon.IndexBlock, difference);

                        indexBlock += difference;
                        fileBlock += difference;
                    }

                    // Now that there is enough space to write the metadata, write it
                    stream.Seek(0, SeekOrigin.Begin);
                    metadataBlock.Seek(0, SeekOrigin.Begin);
                    metadataBlock.CopyTo(stream);
                }

                // --- Index block ---                
                SortedList<uint, KeyValuePair<ContentFile, IndexEntry>> fileUpdates =
                    new SortedList<uint, KeyValuePair<ContentFile,IndexEntry>>();
                {
                    long Offset = 0; // A rolling variable which contains where a file pointed by the current index begins (relative to fileblock's begin)
                    stream.Seek(indexBlock, SeekOrigin.Begin); // After moving the metadata, the index block begins here
                    long previousFileIndexEntryPosition = indexBlock;

                    uint index = 1;
                    foreach (ContentFile f in rAddon.GetFiles())
                    {
                        stream.Seek(previousFileIndexEntryPosition, SeekOrigin.Begin);
                        long currentFileIndexPosition = stream.Position;

                        // Read the index area for the file
                        int fileNum = br.ReadInt32(); // file number (4)
                        string path = String.Empty; long size = -1; uint CRC = 0;
                        if (fileNum != 0)
                        {
                            // Write the (proper, new) index of the currently read file entry
                            stream.Seek(-4, SeekOrigin.Current);
                            bw.Write(index); // file number (4)

                            path = br.ReadNullTerminatedString();
                            size = br.ReadInt64(); // size of the current file (8)
                            CRC = br.ReadUInt32(); // (4)
                        }
                        long offset = Offset; // offset of the current file
                        long nextFileEntryIndexPosition = stream.Position;
                        
                        if (f.State != ContentFile.FileState.Intact)
                        {
                            // The intact file was already skipped
                            switch (f.State)
                            {
                                case ContentFile.FileState.Modified:
                                    if (size != f.Size)
                                    {
                                        // The new file size differs from the original one
                                        long difference = f.Size - size;

                                        // The index must be updated so that the new file size is stored correctly
                                        stream.Seek(currentFileIndexPosition, SeekOrigin.Begin);
                                        br.ReadInt32(); // skip the file num (4)
                                        br.ReadNullTerminatedString(); // skip the path
                                        bw.Write((long)f.Size); // the new size (8)

                                        // We need to alter the file area to make room for the new file
                                        stream.MoveEndPart(fileBlock + offset + size, difference); // fileBlock + offset + size is the begin position of the next file

                                        size = f.Size; // Change this to the new size so that Offset is updated correctly after the switch{}
                                    }
                                    // Else: the file sizes are the same, the size in index entry is not updated

                                    // Update the CRC of the file
                                    stream.Seek(currentFileIndexPosition, SeekOrigin.Begin);
                                    br.ReadInt32(); // skip the file number
                                    br.ReadNullTerminatedString(); // skip the path
                                    br.ReadInt64(); // skip the file size (8)
                                    bw.Write(f.CRC); // the new crc (4)

                                    // Mark the file for writing
                                    if (!fileUpdates.ContainsKey(index))
                                        fileUpdates.Add(index, new KeyValuePair<ContentFile, IndexEntry>(f,
                                                new IndexEntry(path, f.Size, f.CRC, offset))
                                        );
                                    else
                                        fileUpdates[index] = new KeyValuePair<ContentFile, IndexEntry>(f,
                                            new IndexEntry(path, f.Size, f.CRC, offset));
                                    break;
                                case ContentFile.FileState.Added:
                                    // If a new file is added, we need to make space for it in the index area...
                                    using (MemoryStream indexElement = new MemoryStream())
                                    using (BinaryWriter elemWriter = new BinaryWriter(indexElement))
                                    {
                                        elemWriter.Write(index); // File number (4)
                                        elemWriter.WriteNullTerminatedString(f.Path.ToLowerInvariant()); // File name (all lower case!) (n)
                                        elemWriter.Write((long)f.Size); // File size (8)
                                        elemWriter.Write(f.CRC); // File CRC (4, unsigned)

                                        // currentFile is actually the _NEXT_ file here (so the "current" one in the index of the Stream, not the runtime)
                                        // A move operation here will make place for the added file while essentially copying the remaining index data.
                                        stream.MoveEndPart(currentFileIndexPosition, indexElement.Length);
                                        nextFileEntryIndexPosition = currentFileIndexPosition + indexElement.Length;

                                        fileBlock += indexElement.Length; // The index area has grown so the files start later

                                        // Write the new index entry into the stream
                                        stream.Seek(currentFileIndexPosition, SeekOrigin.Begin);
                                        indexElement.Seek(0, SeekOrigin.Begin);
                                        indexElement.CopyTo(stream);

                                        // The stream is now positioned on the next record (which contains the same data as before the copy because of moving)
                                    }

                                    // ... and we need to make space for it in the file area too
                                    // offset is where the file after the to-be-added one begun
                                    stream.MoveEndPart(fileBlock + offset, f.Size);

                                    size = f.Size; // Make sure the offset after the switch{} is properly modified, the next file should now begin after the current one

                                    // Mark the file for writing
                                    if (!fileUpdates.ContainsKey(index))
                                        fileUpdates.Add(index, new KeyValuePair<ContentFile, IndexEntry>(f,
                                            new IndexEntry(f.Path, f.Size, f.CRC, offset))
                                        );
                                    else
                                        fileUpdates[index] = new KeyValuePair<ContentFile, IndexEntry>(f,
                                            new IndexEntry(f.Path, f.Size, f.CRC, offset));
                                    break;
                                case ContentFile.FileState.Deleted:
                                    // If a file is deleted, its index entry must be removed...
                                    stream.Seek(currentFileIndexPosition, SeekOrigin.Begin);
                                    br.ReadInt32(); // skip file num (4)
                                    br.ReadNullTerminatedString(); // skip path
                                    br.ReadInt64(); // size (8)
                                    br.ReadUInt32(); // crc (4)

                                    long nextEntryAt = stream.Position;
                                    long indexDeleteLength = currentFileIndexPosition - nextEntryAt;

                                    stream.MoveEndPart(nextEntryAt, indexDeleteLength);
                                    fileBlock += indexDeleteLength; // The index area was shrunk, so the file block begins earlier

                                    nextFileEntryIndexPosition = nextEntryAt + indexDeleteLength;

                                    // ... and the contents should be deleted from the file area too
                                    stream.MoveEndPart(fileBlock + offset + size, -size);

                                    --index; // This file is removed so indexing should continue from the current value, not the next one.
                                    size = 0; // To make sure the offset is aligned correctly, we don't need to align it as the next file was moved over to the current one's

                                    break;
                            }
                        }

                        // Advance the pointer in the index area
                        previousFileIndexEntryPosition = nextFileEntryIndexPosition;

                        if (f.State != ContentFile.FileState.Deleted)
                        {
                            if (!Wresults.IndexUpdates.ContainsKey(index))
                                Wresults.IndexUpdates.Add(index, new KeyValuePair<ContentFile, IndexEntry>(f,
                                    new IndexEntry(f.Path, f.Size, f.CRC, offset))
                                );
                            else
                                Wresults.IndexUpdates[index] = new KeyValuePair<ContentFile, IndexEntry>(f,
                                    new IndexEntry(f.Path, f.Size, f.CRC, offset));
                        }

                        ++index;
                        Offset += size; // The next file will begin after the current one
                    }

                    // Write the zero to make sure the index area is closed.
                    stream.Seek(fileBlock - 1, SeekOrigin.Begin);
                    bw.Write(0);
                }

                // --- Files ---
                foreach (KeyValuePair<ContentFile, IndexEntry> update in fileUpdates.Values)
                {
                    // Write the file to the given location
                    stream.Seek(fileBlock + update.Value.Offset, SeekOrigin.Begin);
                    byte[] content = update.Key.Content;

                    if (stream.Position != fileBlock + update.Value.Offset)
                        throw new Exception("Attempted to compile contents of " + update.Key.Path + " to the addon at position " +
                            update.Value.Offset + " (relative to FileBlock at " + fileBlock + "), but the contents of the file " +
                            "was effectively read from that addon.\n\nThis exception indicates an EMERGENCY FAILURE of the business logic." +
                            "Execution CAN NOT continue.");

                    stream.Write(content, 0, (int)update.Key.Size);
                }
                stream.Flush();

                // --- CRC ---
                stream.Seek(-4, SeekOrigin.End);
                uint addonCRC = br.ReadUInt32();

                // After everything has been modified, we need to re-CRC the addon
                stream.Seek(0, SeekOrigin.Begin);
                byte[] buffer_whole = new byte[br.BaseStream.Length - 4];
                stream.Read(buffer_whole, 0, (int)stream.Length - 4);
                uint calculatedCRC = System.Cryptography.CRC32.ComputeChecksum(buffer_whole);

                bw.Seek(-4, SeekOrigin.End);
                bw.Write(calculatedCRC);

                // End.
                Wresults.NewIndexBlock = (ulong)indexBlock;
                Wresults.NewFileBlock = (ulong)fileBlock;
                return Wresults;
            }
        }
    }
}
