using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SharpGMad
{
    static class Output
    {
        public static void Warning(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ResetColor();
        }
    }

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
                    int folderIndex = Array.FindIndex(args, a => a == "-folder");
                    
                    if ( folderIndex == -1 )
                        throw new Exception(); // This means that the switch does not exist
                    else
                        strFolder = args[Array.FindIndex(args, a => a == "-folder") + 1];
                }
                catch (Exception)
                {
                    strFolder = "";
                }
                if (strFolder == "")
                {
                    Console.WriteLine("Missing -folder (the folder to turn into an addon)");
                    return 1;
                }

                string strTarget;
                try
                {
                    int targetIndex = Array.FindIndex(args, a => a == "-out");

                    if (targetIndex == -1)
                        throw new Exception(); // This means that the switch does not exist
                    else
                        strTarget = args[Array.FindIndex(args, a => a == "-out") + 1];
                }
                catch (Exception)
                {
                    strTarget = "";
                }
                if (strTarget == "")
                {
                    Console.WriteLine("Missing -out (the filename of the target gma)");
                    return 1;
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
                    int fileIndex = Array.FindIndex(args, a => a == "-file");

                    if (fileIndex == -1)
                        throw new Exception(); // This means that the switch does not exist
                    else
                        strFile = args[Array.FindIndex(args, a => a == "-file") + 1];
                }
                catch (Exception)
                {
                    strFile = "";
                }
                if (strFile == "")
                {
                    Console.WriteLine("Missing -file (the addon you want to extract)");
                    return 1;
                }

                string strTarget;
                try
                {
                    int targetIndex = Array.FindIndex(args, a => a == "-out");

                    if (targetIndex == -1)
                        throw new Exception(); // This means that the switch does not exist
                    else
                        strTarget = args[Array.FindIndex(args, a => a == "-out") + 1];
                }
                catch (Exception)
                {
                    strTarget = "";
                }

                return ExtractAddonFile(strFile, strTarget);
            }

            //
            // Help
            //
            Console.WriteLine("Usage:");
            Console.WriteLine();

            return 0;
        }

        public static int CreateAddonFile(string strFolder, string strOutfile, bool warnInvalid)
        {
            //bool bErrors = false;
            //
            // Make sure there's a slash on the end
            //
            strFolder = strFolder.TrimEnd('/');
            strFolder = strFolder + "/";
            //
            // Make sure OutFile ends in .gma
            //
            strOutfile = Path.GetFileNameWithoutExtension(strOutfile);
            strOutfile += ".gma";
            Console.WriteLine("Looking in folder \"" + strFolder + "\"");
            //
            // Load the Addon Info file
            //
            Addon.Json addonInfo = new Addon.Json(strFolder + "addon.json");

            if (addonInfo.GetError() != String.Empty && addonInfo.GetError() != null)
            {
                Output.Warning(strFolder + "addon.json" + " error: " + addonInfo.GetError());
                return 1;
            }

            //
            // Get a list of files in the specified folder
            //
            List<string> files = new List<string>();
            foreach (string f in Directory.GetFiles(strFolder, "*", SearchOption.AllDirectories))
            {
                string file = f;
                file = file.Replace(strFolder, String.Empty);
                file = file.Replace('\\', '/');
                
                files.Add(file);
            }
            //
            // Let the addon json remove the ignored files
            //
            addonInfo.RemoveIgnoredFiles(ref files);
            //
            // Sort the list into alphabetical order, no real reason - we're just ODC
            //
            files.Sort();

            //
            // Verify
            //
            if (!CreateAddon.VerifyFiles(ref files, warnInvalid))
            {
                Output.Warning("File list verification failed");
                return 1;
            }

            //
            // Create an addon file in a buffer
            //
            MemoryStream buffer = new MemoryStream();

            if (!CreateAddon.Create(ref buffer, strFolder, ref files, addonInfo.GetTitle(), addonInfo.BuildDescription()))
            {
                Output.Warning("Failed to create the addon");
                return 1;
            }

            //
            // Save the buffer to the provided name
            //
            buffer.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[buffer.Length];
            buffer.Read(bytes, 0, (int)buffer.Length);

            try
            {
                File.WriteAllBytes(strOutfile, bytes);
            }
            catch (Exception)
            {
                Output.Warning("Couldn't save to file \"" + strOutfile + "\"");
                return 1;
            }

            //
            // Success!
            //
            Console.WriteLine("Successfully saved to \"" + strOutfile + "\" [" + Memory((int)buffer.Length) + "]");
            return 0;
        }

        static int ExtractAddonFile(string strFile, string strOutPath = "")
        {
            Console.WriteLine("Opening " + strFile);

            //
            // If an out path hasn't been provided, make our own
            //
            if (strOutPath == String.Empty)
            {
                strOutPath = Path.GetFileNameWithoutExtension(strFile);
            }

            //
            // Remove slash, add slash (enforces a slash)
            //
            strOutPath = strOutPath.TrimEnd('/');
            strOutPath = strOutPath + '/';
            Addon.Reader addon = new Addon.Reader();

            if (!addon.ReadFromFile(strFile))
            {
                Output.Warning("There was a problem opening the file");
                return 1;
            }

            if (!addon.Parse())
            {
                Output.Warning("There was a problem parsing the file");
                return 1;
            }

            Console.WriteLine("Extracting Files:");
            foreach ( Addon.Format.FileEntry entry in addon.GetList())
            {
                Console.WriteLine("\t" + entry.strName + " [" + Memory((int)entry.iSize) + "]");
                // Make sure folder exists
                try
                {
                    Directory.CreateDirectory(strOutPath + Path.GetDirectoryName(entry.strName));
                }
                catch (Exception)
                {
                    // Noop
                }
                // Load the file into the buffer
                using (MemoryStream filecontents = new MemoryStream())
                {
                    if ( addon.ReadFile(entry.iFileNumber, filecontents) )
                    {
                        using(FileStream file = new FileStream(strOutPath + entry.strName, FileMode.Create, FileAccess.Write))
                        {
                            filecontents.Seek(0, SeekOrigin.Begin);
                            filecontents.CopyTo(file);
                        }
                    }
                    else
                    {
                        Output.Warning("\t\tCouldn't extract!");
                    }
                }
            }
            Console.WriteLine("Done!");
            return 0;
        }

        public static string Memory(int iBytes)
        {
            float gb = iBytes / (float)1024 / (float)1024 / (float)1024;

            if (gb >= 1.0)
            {
                return String.Format("{0:0.##} {1}", gb, "GiB");
            }

            float mb = iBytes / (float)1024 / (float)1024;

            if (mb >= 1.0)
            {
                return String.Format("{0:0.##} {1}", mb, "MiB");
            }

            float kb = iBytes / (float)1024;

            if (kb >= 1.0)
            {
                return String.Format("{0:0.##} {1}", kb, "KiB");
            }

            // return as bytes
            return iBytes + " B";
        }

    }
} 