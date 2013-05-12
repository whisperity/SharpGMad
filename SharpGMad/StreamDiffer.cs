using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGMad
{
    class StreamDiffer
    {
        private struct DiffRange
        {
            public ulong iPosition;
            public uint iLength;
            public List<ByteDiff> bContent;
            public bool EndOfStream;
        }

        private struct ByteDiff
        {
            public byte Value { get; private set; }
            public bool EndOfStream { get; private set; }

            public ByteDiff(byte value) : this()
            {
                Value = value;
                EndOfStream = false;
            }

            public ByteDiff(bool endOfStream) : this()
            {
                Value = 0;
                EndOfStream = true;
            }
        }

        public Stream Storage; // This is the backend stream (e.g: a file)
        public Stream Volatile; // This is the "buffer" stream, the one that gets written to

        public StreamDiffer(Stream storage)
        {
            Storage = storage;

            Volatile = new MemoryStream();

            Storage.Seek(0, SeekOrigin.Begin);
            Storage.CopyTo(Volatile);
        }

        public void Write(byte[] buffer, int bufferOffset, int count, int streamOffset)
        {
            Volatile.Seek(streamOffset, SeekOrigin.Begin);
            Volatile.Write(buffer, bufferOffset, count);
        }

        public void Write(Stream stream)
        {
            Volatile.Seek(0, SeekOrigin.Begin);
            Volatile.SetLength(0);

            stream.Seek(0, SeekOrigin.Begin);
            
            stream.CopyTo(Volatile);
        }

        public void Push()
        {
            /*Console.WriteLine("Storage " + Storage.Length + " bytes against volatile " +
                                Volatile.Length + " bytes.");*/

            Storage.Seek(0, SeekOrigin.Begin);
            Volatile.Seek(0, SeekOrigin.Begin);

            byte Porig = 0;
            byte Ptmp = 0;

            Dictionary<long, ByteDiff> differences = new Dictionary<long, ByteDiff>(
                (int)(Storage.Length > Volatile.Length ? Storage.Length : Volatile.Length));

            bool eOrig = false;
            bool eTmp = false;

            int iOrig = 0;
            int iTmp = 0;

            for (long i = 0; i <= (Storage.Length > Volatile.Length ? Storage.Length : Volatile.Length); i++)
            {
                iOrig = Storage.ReadByte();
                if (iOrig == -1 || Storage.Position > Storage.Length)
                    eOrig = true;
                else
                    Porig = (byte)iOrig;

                iTmp = Volatile.ReadByte();
                if (iTmp == -1 || Volatile.Position > Volatile.Length)
                    eTmp = true;
                else
                    Ptmp = (byte)iTmp;

                // original stream is already over
                // there's still data in buffer stream
                if (eOrig && !eTmp)
                {
                    //Console.WriteLine("At " + i + " NULL -> " + Convert.ToChar(Ptmp));
                    differences.Add(i, new ByteDiff(Ptmp));
                    continue;
                }

                // Original still has data, but buffer is over
                if (!eOrig && eTmp)
                {
                    //Console.WriteLine("At " + i + " " + Convert.ToChar(Porig) + " -> NULL");
                    differences.Add(i, new ByteDiff(true));
                    continue;
                }

                // Original and buffer has the same value: no difference
                if (Porig == Ptmp)
                {
                    continue;
                }

                // There is a difference
                if (Porig != Ptmp)
                {
                    //Console.WriteLine("At " + i + " " + Convert.ToChar(Porig) + " -> " + Convert.ToChar(Ptmp));
                    differences.Add(i, new ByteDiff(Ptmp));
                    continue;
                }
            }

            Console.WriteLine(differences.Count + " bytes differ.");
            if (differences.Count != 0)
            {
                Console.WriteLine("Analyzing diff, creating chunks.");

                List<DiffRange> chunks = new List<DiffRange>(differences.Count);

                DiffRange one_diff = new DiffRange();
                ulong lastPos = 0;
                bool isChunkEnd = false;

                foreach (KeyValuePair<long, ByteDiff> diff in differences)
                {
                    // Check whether we are at a chunk end
                    if (lastPos + 1 == (ulong)diff.Key)
                    {
                        isChunkEnd = false;
                    }
                    else if (lastPos + 1 != (ulong)diff.Key)
                    {
                        isChunkEnd = true;
                    }

                    if (isChunkEnd)
                    {
                        // If we are at the end of a chunk
                        if (lastPos != 0) // If we are not making the first chunk
                        {
                            /*Console.WriteLine("Created chunk " + one_diff.iPosition + " -> " +
                                (one_diff.iPosition + one_diff.iLength - 1));*/
                            chunks.Add(one_diff); // Add previous chunk to list
                        }

                        one_diff = new DiffRange(); // Create new chunk

                        // Set first diff of new chunk up
                        one_diff.iPosition = (ulong)diff.Key;
                        one_diff.iLength = 1;
                        one_diff.bContent = new List<ByteDiff>();
                        if (diff.Value.EndOfStream)
                            one_diff.EndOfStream = true;
                        one_diff.bContent.Add(diff.Value);

                        lastPos = (ulong)diff.Key;
                        continue;
                    }
                    else
                    {
                        // The current diff byte is sequent to the undergoing chunk
                        // just add the value to the chunk and advance
                        one_diff.iLength++;
                        if (diff.Value.EndOfStream)
                            one_diff.EndOfStream = true;
                        one_diff.bContent.Add(diff.Value);

                        lastPos = (ulong)diff.Key;

                        continue;
                    }
                }

                // Add the last chunk we created in the foreach to the list too
                if (one_diff.iLength != 0)
                    chunks.Add(one_diff);

                for (int i = 0; i <= (chunks.Count - 1); i++)
                {
                    DiffRange one_chunk = chunks[i];
                    /*Console.WriteLine("Updating chunk " + one_chunk.iPosition + " -> " +
                        (one_chunk.iPosition + one_chunk.iLength - 1));*/

                    /*byte[] value = one_chunk.bContent.Select(s => s.Value).ToArray();

                    Console.WriteLine("Chunk from " + one_chunk.iPosition + " to " +
                        (one_chunk.iPosition + one_chunk.iLength) + ": " +
                        Encoding.ASCII.GetString(value));*/
                    Storage.Seek((long)one_chunk.iPosition, SeekOrigin.Begin);
                    Storage.Write(one_chunk.bContent.Select(s => s.Value).ToArray(), 0, (int)one_chunk.iLength);
                }
                // If the last chunk contains information about stream truncation at the end
                // we must shorten the length of the original stream so these bytes
                // are properly cut off.
                if (chunks.Count > 0)
                {
                    DiffRange lastChunk = chunks[chunks.Count - 1];
                    if (lastChunk.iPosition == (ulong)Storage.Length - (ulong)lastChunk.iLength)
                    {
                        //Console.WriteLine("Is ending chunk.");

                        // We set the original ending location to the very end of the chunk
                        uint endLocation = (uint)lastChunk.iPosition + lastChunk.iLength;

                        // Going from end to beginning, iterate the chunk's content
                        for (int i = (lastChunk.bContent.Count - 1); i >= 0; i--)
                        {
                            // Set the end location of the stream itself
                            // (the count is the List<byte>'s count!)
                            if (lastChunk.bContent[i].EndOfStream == true)
                            {
                                endLocation = (uint)i;
                            }
                        }

                        /*if (lastChunk.EndOfStream == true)
                        {
                            Console.WriteLine("Stream was over too.");
                            Console.WriteLine("Will truncate " + (lastChunk.iLength - endLocation) + " bytes.");
                        }*/

                        byte[] real_content = lastChunk.bContent.Take((int)endLocation).Select(s => s.Value).ToArray();

                        Storage.Seek((long)lastChunk.iPosition, SeekOrigin.Begin);
                        Storage.Write(real_content, 0, real_content.Length);
                        Storage.SetLength(Storage.Position);
                    }
                }
                Storage.Flush();

                //Console.WriteLine("Original was synced, " + differences.Count + " bytes rewritten.");
            }
        }
    }
}
