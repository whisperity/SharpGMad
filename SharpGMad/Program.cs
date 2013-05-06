using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpGMad
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Garry's Addon Creator 1.0");
            Console.ResetColor();

            //
            // Get the command from the command line - (it should be argument 0)
            //
            string strCommand;
            try
            {
                strCommand = args[0];
            }
            catch (IndexOutOfRangeException)
            {
                strCommand = "";
            }

            //
            // Create
            //
            if (strCommand == "create")
            {
                string strFolder;
                try
                {
                    strFolder = args[Array.FindIndex(args, a => a == "-folder") + 1];
                }
                catch (IndexOutOfRangeException)
                {
                    strFolder = "";
                }
                if (strFolder == "")
                {
                    Console.WriteLine("Missing -folder (the folder to turn into an addon)");
                    Environment.Exit(1);
                }

                string strTarget;
                try
                {
                    strTarget = args[Array.FindIndex(args, a => a == "-out") + 1];
                }
                catch (IndexOutOfRangeException)
                {
                    strTarget = "";
                }
                if (strTarget == "")
                {
                    Console.WriteLine("Missing -out (the filename of the target gma)");
                    Environment.Exit(1);
                }

                bool WarnOnInvalidFiles = args.Contains("-warninvalid");

                return CreateAddonFile(strFolder, strTarget, WarnOnInvalidFiles);
            }

            //
            // Extract
            //
            if (strCommand == "extract")
            {
                string strFile;
                try
                {
                    strFile = args[Array.FindIndex(args, a => a == "-file") + 1];
                }
                catch (IndexOutOfRangeException)
                {
                    strFile = "";
                }
                if (strFile == "")
                {
                    Console.WriteLine("Missing -file (the addon you want to extract)");
                    Environment.Exit(1);
                }

                string strTarget;
                try
                {
                    strTarget = args[Array.FindIndex(args, a => a == "-out") + 1];
                }
                catch (IndexOutOfRangeException)
                {
                    strTarget = "";
                }
                if (strTarget == "")
                {
                    Console.WriteLine("Missing -out (the filename of the target gma)");
                    Environment.Exit(1);
                }

                return ExtractAddonFile(strFile, strTarget);
            }

            //
            // Help
            //
            Console.WriteLine("Usage:");
            Console.WriteLine();

            Console.WriteLine(Addon.Whitelist.Check("a.exe"));
            Console.WriteLine(Addon.Format.TimestampOffset);

            Console.ReadLine();
            return 0;
        }

        static int CreateAddonFile(string strFolder, string strOutfile, bool warnInvalid)
        {
            return 0;
        }

        static int ExtractAddonFile(string strFile, string strOutPath)
        {
            return 0;
        }
    }
}

namespace Addon
{
    class Whitelist
    {
        private static string[] Wildcard = new string[]{
            "maps/*.bsp",
			"maps/*.png",
			"maps/*.nav",
			"maps/*.ain",
			"sound/*.wav",
			"sound/*.mp3",
			"lua/*.lua",
			"materials/*.vmt",
			"materials/*.vtf",
			"materials/*.png",
			"models/*.mdl",
			"models/*.vtx",
			"models/*.phy",
			"models/*.ani",
			"models/*.vvd",
			"gamemodes/*.txt",
			"gamemodes/*.lua",
			"scenes/*.vcd",
			"particles/*.pcf",
			"gamemodes/*/backgrounds/*.jpg",
			"gamemodes/*/icon24.png",
			"gamemodes/*/logo.png",
			"scripts/vehicles/*.txt",
			"resource/fonts/*.ttf",
			null
        };

        //
        // Call on a filename including relative path to determine
        // whether file is allowed to be in the addon.
        //
        public static bool Check(string strName)
        {
            bool bValid = false;

            for (int i = 0; ; i++)
            {
                if (bValid || Wildcard[i] == null)
                {
                    break;
                }

                bValid = TestWildcard(Wildcard[i], strName);
            }

            return bValid;
        }

        private static bool TestWildcard(string wildcard, string strName)
        {
            string pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return (regex.IsMatch(strName));
        }
    }

    class Format
    {
        public const string Ident = "GMAD";
        public const char Version = '3';
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
            string strName;
            long iSize;
            ulong iCRC;
            uint iFileNumber;
            long iOffset;

            List<FileEntry> List;
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

    class Reader
    {
        public Reader()
        {
            //Clear();
        }

        //
        // Load an addon (call Parse after this succeeds)
        //
        /*public bool ReadFromFile(string strName)
        {
            
        }*/

        protected AutoBuffer m_buffer;
        protected char m_fmtversion;
        protected string m_name;
        protected string m_author;
        protected string m_desc;
        protected string m_type;
        protected List<Format.FileEntry> m_index;
        protected ulong m_fileblock;

        List<string> m_tags;
    }
}

class AutoBuffer
{

}
    