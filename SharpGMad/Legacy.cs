using System;
using System.IO;
using System.Linq;

namespace SharpGMad
{
    /// <summary>
    /// Provides methods for the legacy interface of gmad.exe.
    /// </summary>
    class Legacy
    {
        /// <summary>
        /// The main entry point for the legacy interface.
        /// </summary>
        public static int Main(string[] args)
        {
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

                    if (folderIndex == -1)
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

            return 0;
        }

        /// <summary>
        /// Legacy GMAD operation to create an addon file using contents of a specified folder.
        /// </summary>
        /// <param name="strFolder">The folder containing the raw content.</param>
        /// <param name="strOutfile">The path of the addon file to write.</param>
        /// <param name="warnInvalid">Whether there should be a warning for files failing to validate
        /// instead of a full exception halt.</param>
        /// <returns>Integer error code: 0 if success, 1 if error.</returns>
        static int CreateAddonFile(string strFolder, string strOutfile, bool warnInvalid)
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
            Json addonInfo;
            try
            {
                addonInfo = new Json(strFolder + "addon.json");
            }
            catch (AddonJSONException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(strFolder + "addon.json error: " + ex.Message);
                Console.ResetColor();
                return 1;
            }

            Addon addon = new Addon(addonInfo);

            //
            // Get a list of files in the specified folder
            //
            foreach (string f in Directory.GetFiles(strFolder, "*", SearchOption.AllDirectories))
            {
                string file = f;
                file = file.Replace(strFolder, String.Empty);
                file = file.Replace('\\', '/');

                Console.WriteLine("\t" + file);

                try
                {
                    addon.AddFile(file, File.ReadAllBytes(f));
                }
                catch (IOException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unable to read file " + file);
                    Console.ResetColor();
                    continue;
                }
                catch (IgnoredException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t\t[Ignored]");
                    Console.ResetColor();
                    continue;
                }
                catch (WhitelistException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t\t[Not allowed by whitelist]");
                    Console.ResetColor();
                    if (!warnInvalid)
                        return 1;
                }
            }
            //
            // Let the addon json remove the ignored files
            //
            //addonInfo.RemoveIgnoredFiles(ref files);
            //
            // Sort the list into alphabetical order, no real reason - we're just ODC
            //
            addon.Sort();

            //
            // Create an addon file in a buffer
            //
            MemoryStream buffer;
            try
            {
                Writer.Create(addon, out buffer);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to create the addon");
                Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Couldn't save to file \"" + strOutfile + "\"");
                Console.ResetColor();
                return 1;
            }

            //
            // Success!
            //
            Console.WriteLine("Successfully saved to \"" + strOutfile + "\" [" + ((int)buffer.Length).HumanReadableSize() + "]");
            return 0;
        }

        /// <summary>
        /// Legacy GMAD operation to extract an addon file to a specified folder.
        /// </summary>
        /// <param name="strFile">The file path of the GMA to extract.</param>
        /// <param name="strOutPath">The folder where the addon is to be extracted to.</param>
        /// <returns>Integer error code: 0 if success, 1 if error.</returns>
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
            Addon addon;
            try
            {
                addon = new Addon(new Reader(strFile));
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem opening or parsing the file");
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine("Extracting Files:");
            foreach (ContentFile entry in addon.Files)
            {
                Console.WriteLine("\t" + entry.Path + " [" + ((int)entry.Size).HumanReadableSize() + "]");
                // Make sure folder exists
                try
                {
                    Directory.CreateDirectory(strOutPath + Path.GetDirectoryName(entry.Path));
                }
                catch (Exception)
                {
                    // Noop
                }
                // Write the file to the disk
                try
                {
                    using (FileStream file = new FileStream(strOutPath + entry.Path, FileMode.Create, FileAccess.Write))
                    {
                        file.Write(entry.Content, 0, (int)entry.Size);
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t\tCouldn't extract!");
                    Console.ResetColor();
                }
            }
            Console.WriteLine("Done!");
            return 0;
        }
    }
}