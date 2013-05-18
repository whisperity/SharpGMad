using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    class Addon
    {
        // Static members: format setup.
        public const string Ident = "GMAD";
        public const char Version = (char)3;
        public const uint AppID = 4000;
        public const uint CompressionSignature = 0xBEEFCACE;

        private struct Header
        {
            public string Ident;
            public char Version;

            public Header(string ident, char version)
            {
                this.Ident = ident;
                this.Version = version;
            }
        }

        //
        // This is the position in the file containing a 64 bit unsigned int that represents the file's age
        // It's basically the time it was uploaded to Steam - and is set on download/extraction from steam.
        //
        public static uint TimestampOffset = (uint)System.Runtime.InteropServices.Marshal.SizeOf(new Header(Ident, Version))
            + (uint)sizeof(ulong);

        // Instance members
        public string Name;
        public string Author;
        public string Description;
        public string Type;
        public List<FileEntry> Files;
        public List<string> Tags;
    }

    struct FileEntry
    {
        public string Path;
        public long Size;
        public ulong CRC;
        public uint FileNumber;
        public long Offset;
        public byte[] Content;
    }

    static class Tags
    {
        // Only one of these
        private static string[] Type = new string[]{
            "gamemode",
            "map",
            "weapon",
            "vehicle",
            "npc",
            "tool",
            "effects",
            "model"
        };

        public static short TypeCount { get { return (short)Type.Count(); } }

        public static bool TypeExists(string type) { return Type.Contains(type); }

        // Up to two of these
        private static string[] Misc = new string[]{
            "fun",
            "roleplay",
            "scenic",
            "movie",
            "realism",
            "cartoon",
            "water",
            "comic",
            "build"
        };

        public static short MiscCount { get { return (short)Misc.Count(); } }

        public static bool TagExists(string tag) { return Misc.Contains(tag); }
    }
}
