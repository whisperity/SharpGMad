using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpGMad
{
    /// <summary>
    /// Provides method for converting old (GMod 12) addon structures into GMA files.
    /// </summary>
    class GMod12Pack
    {
        /// <summary>
        /// The main entry point for the conversion interface.
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
            // Convert
            //
            if (strCommand == "convert")
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

                return ConvertAddonFolder(strFolder, strTarget, WarnOnInvalidFiles);
            }

            return 0;
        }

        /// <summary>
        /// Converts the specified folder into a GMA file.
        /// </summary>
        /// <param name="strFolder">The folder containing the raw content.</param>
        /// <param name="strOutfile">The path of the addon file to write.</param>
        /// <param name="warnInvalid">Whether there should be a warning for files failing to validate
        /// instead of a full exception halt.</param>
        /// <returns>Integer error code: 0 if success, 1 if error.</returns>
        static int ConvertAddonFolder(string strFolder, string strOutfile, bool warnInvalid)
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
            
            // Load the addon metadata from the old file structure: info.txt or addon.txt.
            string legacyInfoFile;
            if ( File.Exists(strFolder + "\\info.txt") )
                legacyInfoFile = "info.txt";
            else if (File.Exists(strFolder + "\\addon.txt"))
                legacyInfoFile = "addon.txt";
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to find legacy addon metadata file \"info.txt\" or \"addon.txt\"");
                Console.ResetColor();
                return 1;
            }

            string legacyInfo;
            try
            {
                legacyInfo = File.ReadAllText(strFolder + Path.DirectorySeparatorChar + legacyInfoFile);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to read metadata.");
                Console.ResetColor();
                Console.WriteLine(ex.Message);
                return 1;
            }

            Addon addon = new Addon();

            // Parse the read data
            Regex regex = new System.Text.RegularExpressions.Regex("\"([A-Za-z_\r\n])*\"", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(legacyInfo);

            foreach (Match keyMatch in matches)
            {
                if (keyMatch.Value.ToLowerInvariant() == "\"name\"")
                    addon.Title = keyMatch.NextMatch().Value;
                else if (keyMatch.Value.ToLowerInvariant() == "\"info\"")
                    addon.Description = keyMatch.NextMatch().Value;
                else if (keyMatch.Value.ToLowerInvariant() == "\"author_name\"")
                    addon.Author = keyMatch.NextMatch().Value;
                    // Current GMAD writer only writes "Author Name", not real value
            }

            Console.WriteLine(addon.Title + " by " + addon.Author);
            Console.WriteLine("You need to set the title, and optionally, the tags for this addon!");

            RealtimeCommandline.SetType(addon);
            RealtimeCommandline.SetTags(addon);

            Console.WriteLine("Adding files...");

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
            //
            // Save the buffer to the provided name
            //
            FileStream gmaFS;
            try
            {
                gmaFS = new FileStream(strOutfile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                gmaFS.SetLength(0); // Truncate the file

                Writer.Create(addon, gmaFS);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to create the addon");
                Console.ResetColor();
                return 1;
            }
            
            //
            // Success!
            //
            Console.WriteLine("Successfully saved to \"" + strOutfile + "\" [" + ((int)gmaFS.Length).HumanReadableSize() + "]");
            gmaFS.Flush();
            gmaFS.Dispose();
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
                FileStream fs = new FileStream(strFile, FileMode.Open, FileAccess.ReadWrite);
                addon = new Addon(new Reader(fs));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem opening or parsing the file");
                Console.ResetColor();
                Console.WriteLine(ex.Message);
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