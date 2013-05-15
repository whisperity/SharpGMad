using System.Collections.Generic;
using System.Linq;

namespace Addon
{
    public static class Format
    {
        public const string Ident = "GMAD";
        public const char Version = (char)3;
        public const uint AppID = 4000;
        public const uint CompressionSignature = 0xBEEFCACE;

        public struct Header
        {
            string Ident;
            char Version;

            public Header(string ident, char version)
            {
                this.Ident = ident;
                this.Version = version;
            }
        }

        public struct FileEntry
        {
            public string strName;
            public long iSize;
            public ulong iCRC;
            public uint iFileNumber;
            public long iOffset;
        }

        //
        // A list of tags that are available to set
        //
        public class Tags
        {
            //
            // Only one of these
            //
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

            static short TypesCount = (short)Type.Count();

            public static bool TypeExists(string strName)
            {
                for (int i = 0; i < TypesCount; i++)
                {
                    if (strName == Type[i])
                        return true;
                }
                return false;
            }

            //
            // Up to two of these
            //
            static string[] Misc = new string[]{
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

            static short MiscCount = (short)Misc.Count();

            public static bool TagExists(string strName)
            {
                for (int i = 0; i < MiscCount; i++)
                {
                    if (strName == Misc[i])
                        return true;
                }

                return false;
            }
        }

        //
        // This is the position in the file containing a 64 bit unsigned int that represents the file's age
        // It's basically the time it was uploaded to Steam - and is set on download/extraction from steam.
        //
        public static uint TimestampOffset = (uint)System.Runtime.InteropServices.Marshal.SizeOf(new Header(Ident, Version))
            + (uint)sizeof(ulong);
    }

}
