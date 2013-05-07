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