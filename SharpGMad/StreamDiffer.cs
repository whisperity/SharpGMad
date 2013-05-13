using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGMad
{
    /// <summary>
    /// Provides an object to differentiate and update two System.IO.Stream objects.
    /// </summary>
    public class StreamDiffer
    {
        /// <summary>
        /// Contains information about a difference of a byte in a stream.
        /// </summary>
        private struct ByteDiff
        {
            /// <summary>
            /// Gets the value of the new byte.
            /// </summary>
            public byte Value { get; private set; }
            /// <summary>
            /// Gets whether the difference indicates that the stream was over.
            /// </summary>
            public bool EndOfStream { get; private set; }

            /// <summary>
            /// Creates a new ByteDiff specifying the new byte.
            /// </summary>
            /// <param name="value">The new byte itself.</param>
            public ByteDiff(byte value)
                : this()
            {
                Value = value;
                EndOfStream = false;
            }

            /// <summary>
            /// Create a new ByteDiff marking the end of stream.
            /// </summary>
            /// <param name="endOfStream">Whether the stream has ended. This is the boolean true in this case.</param>
            public ByteDiff(bool endOfStream)
                : this()
            {
                Value = 0;
                EndOfStream = true;
            }
        }

        /// <summary>
        /// A chunk of byte difference.
        /// </summary>
        private struct DiffRange
        {
            /// <summary>
            /// Gets or sets the starting point of difference in the stream.
            /// </summary>
            public ulong InitialPosition { get; set; }
            /// <summary>
            /// Gets or sets the length of the differencing bytes.
            /// </summary>
            public uint Length { get; set; }
            /// <summary>
            /// Contains the list of differencing bytes as ByteDiff objects.
            /// </summary>
            public List<ByteDiff> Bytes { get; set; }
            /// <summary>
            /// Gets or sets whether the chunk is marking after the end of stream.
            /// </summary>
            public bool EndOfStream { get; set; }
        }

        /// <summary>
        /// The stream which is differentially updated by the StreamDiffer.
        /// </summary>
        private Stream Backend;
        /// <summary>
        /// The stream which is accessed by external code.
        /// </summary>
        private Stream Frontend;

        /// <summary>
        /// Initializes a new instance of StreamDiffer using the specified storage stream.
        /// </summary>
        /// <param name="backend">The external stream to update.</param>
        public StreamDiffer(Stream backend)
        {
            Backend = backend;

            Frontend = new MemoryStream();

            Backend.Seek(0, SeekOrigin.Begin);
            Backend.CopyTo(Frontend);
        }

        /// <summary>
        /// Replaces the contents of the internal stream by copying all bytes from the specified Stream into it.
        /// </summary>
        /// <param name="stream">The stream which will have its content copied.</param>
        public void Write(Stream stream)
        {
            Frontend.Seek(0, SeekOrigin.Begin);
            Frontend.SetLength(0);

            stream.Seek(0, SeekOrigin.Begin);

            stream.CopyTo(Frontend);
        }

        /// <summary>
        /// Updates the external stream, using byte difference chunks from the internal buffer
        /// and returns the number of bytes which was updated.
        /// </summary>
        public int Push()
        {
            Backend.Seek(0, SeekOrigin.Begin);
            Frontend.Seek(0, SeekOrigin.Begin);

            int iBackend = 0;
            int iFrontend = 0;
            byte bBackend = 0;
            byte bFrontend = 0;

            bool eosBackend = false;
            bool eosFrontend = false;

            Dictionary<long, ByteDiff> differences = new Dictionary<long, ByteDiff>(
                (int)(Backend.Length > Frontend.Length ? Backend.Length : Frontend.Length));

            // Analyze the streams.
            for (long i = 0; i <= (Backend.Length > Frontend.Length ? Backend.Length : Frontend.Length); i++)
            {
                // Read a byte from backend stream and check whether it's end of stream.
                iBackend = Backend.ReadByte();
                if (iBackend == -1 || Backend.Position > Backend.Length)
                    eosBackend = true;
                else
                    bBackend = (byte)iBackend;

                // Do the same for the frontend stream.
                iFrontend = Frontend.ReadByte();
                if (iFrontend == -1 || Frontend.Position > Frontend.Length)
                    eosFrontend = true;
                else
                    bFrontend = (byte)iFrontend;

                // Backend stream is already over, but
                // there's still data in frontent stream
                if (eosBackend && !eosFrontend)
                {
                    differences.Add(i, new ByteDiff(bFrontend)); // Mark a difference
                    continue;
                }

                // Backend still has data, but frontend is over
                if (!eosBackend && eosFrontend)
                {
                    differences.Add(i, new ByteDiff(true)); // Mark that the stream has ended
                    continue;
                }

                // No difference
                if (bBackend == bFrontend)
                    continue;

                // Both stream are still not at end, but there is a diffing byte.
                if (bBackend != bFrontend)
                {
                    differences.Add(i, new ByteDiff(bFrontend)); // Mark a difference.
                    continue;
                }
            }

            if (differences.Count != 0)
            {
                // Create a list of chunks: subsequent differencing bytes in the stream.
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
                            chunks.Add(one_diff); // Add previous chunk to list

                        one_diff = new DiffRange(); // Create new chunk

                        // Set first diff of new chunk up
                        one_diff.InitialPosition = (ulong)diff.Key;
                        one_diff.Length = 1;
                        one_diff.Bytes = new List<ByteDiff>();
                        one_diff.Bytes.Add(diff.Value);

                        if (diff.Value.EndOfStream)
                            one_diff.EndOfStream = true;

                        lastPos = (ulong)diff.Key;
                        continue;
                    }
                    else
                    {
                        // The current diff byte is sequent to the undergoing chunk
                        // just add the value to the chunk and advance
                        one_diff.Length++;
                        one_diff.Bytes.Add(diff.Value);


                        if (diff.Value.EndOfStream)
                            one_diff.EndOfStream = true;

                        lastPos = (ulong)diff.Key;
                        continue;
                    }
                }

                // Add the last chunk we created in the foreach to the List too
                if (one_diff.Length != 0)
                    chunks.Add(one_diff);

                // Update the backend stream by the chunks
                for (int i = 0; i <= (chunks.Count - 1); i++)
                {
                    Backend.Seek((long)chunks[i].InitialPosition, SeekOrigin.Begin);
                    Backend.Write(chunks[i].Bytes.Select(s => s.Value).ToArray(), 0, (int)chunks[i].Length);
                }

                // If the last chunk contains information about stream truncation at the end
                // we must shorten the length of the original stream so these bytes are properly cut off.
                if (chunks.Count > 0)
                {
                    DiffRange lastChunk = chunks[chunks.Count - 1];
                    if (lastChunk.InitialPosition == (ulong)Backend.Length - (ulong)lastChunk.Length && lastChunk.EndOfStream)
                    {
                        // We set the original ending location to the very end of the chunk
                        uint endLocation = (uint)lastChunk.InitialPosition + lastChunk.Length;

                        // Going from end to beginning, iterate the chunk's content
                        // and search for location of the first "EndOfStream" byte itself.
                        for (int i = (lastChunk.Bytes.Count - 1); i >= 0; i--)
                            if (lastChunk.Bytes[i].EndOfStream == true)
                                endLocation = (uint)i;

                        // Get the real (not EOS) bytes from the stream and update backend.
                        byte[] real_content = lastChunk.Bytes.Take((int)endLocation).Select(s => s.Value).ToArray();

                        Backend.Seek((long)lastChunk.InitialPosition, SeekOrigin.Begin);
                        Backend.Write(real_content, 0, real_content.Length);
                        Backend.SetLength(Backend.Position); // Then truncate the length.
                    }
                }

                // Flush the backend stream to ensure everything is written.
                Backend.Flush();
            }

            return differences.Count;
        }
    }
}
