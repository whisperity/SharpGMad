using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            // Create (or gmod12 convert)
            //
            if (strCommand == "create" || strCommand == "convert")
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

                return CreateAddonFile(strFolder, strTarget, WarnOnInvalidFiles, (strCommand == "convert"));
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
        static int CreateAddonFile(string strFolder, string strOutfile, bool warnInvalid, bool gmod12convert = false)
        {
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

            Addon addon = null;
            if (!gmod12convert)
            {
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

                addon = new Addon(addonInfo);
            }
            else if (gmod12convert)
            {
                // Load the addon metadata from the old file structure: info.txt or addon.txt.
                string legacyInfoFile;
                if (File.Exists(strFolder + "\\info.txt"))
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
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to read metadata.");
                    Console.ResetColor();
                    Console.WriteLine(ex.Message);
                    return 1;
                }

                addon = new Addon();

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

                SetType(addon);
                SetTags(addon);
            }

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
                    addon.CheckRestrictions(file);
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
            // Sort the list into alphabetical order, no real reason - we're just ODC
            //
            addon.Sort();

            //
            // Create an addon file in a buffer
            //
            //
            // Save the buffer to the provided name
            //
            FileStream fs;
            try
            {
                fs = new FileStream(strOutfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Couldn't save to file \"" + strOutfile + "\"");
                Console.ResetColor();
                return 1;
            }

            fs.SetLength(0);
            try
            {
                Writer.Create(addon, fs);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to create the addon.");
                Console.ResetColor();
                return 1;
            }

            fs.Flush();

            //
            // Success!
            //
            Console.WriteLine("Successfully saved to \"" + strOutfile + "\" [" + ((int)fs.Length).HumanReadableSize() + "]");
            fs.Dispose();
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
                FileStream fs = new FileStream(strFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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

        /// <summary>
        /// Sets the type of an addon.
        /// </summary>
        /// <param name="addon">The addon to modify.</param>
        /// <param name="type">Optional. The new type the addon should have.</param>
        private static void SetType(Addon addon, string type = null)
        {
            if (type == String.Empty || type == null)
            {
                while (!Tags.TypeExists(type))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Type? ");
                    Console.ResetColor();
                    Console.Write("Please choose ONE from the following: ");
                    Console.WriteLine(String.Join(" ", Tags.Type));
                    type = Console.ReadLine();

                    if (!Tags.TypeExists(type))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("The specified type is not valid.");
                        Console.ResetColor();
                    }
                }
            }
            else
            {
                if (!Tags.TypeExists(type))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The specified type is not valid.");
                    Console.ResetColor();
                    return;
                }
            }

            addon.Type = type;
        }

        /// <summary>
        /// Sets the tags of an addon.
        /// </summary>
        /// <param name="addon">The addon to modify.</param>
        /// <param name="tagsInput">Optional. The new tags the addon should have.</param>
        private static void SetTags(Addon addon, string[] tagsInput = null)
        {
            List<string> tags = new List<string>(2);
            if (tagsInput == null || tagsInput.Length == 0 || tagsInput[0] == String.Empty)
            {
                bool allTagsValid = false;
                while (!allTagsValid)
                {
                    tags.Clear();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Tags? ");
                    Console.ResetColor();
                    Console.Write("Please choose ZERO, ONE or TWO from the following: ");
                    Console.WriteLine(String.Join(" ", Tags.Misc));

                    tagsInput = Console.ReadLine().Split(' ');

                    allTagsValid = true;
                    if (tagsInput[0] != String.Empty)
                    {
                        // More than zero (one or two) elements: add the first one.
                        if (tagsInput.Length > 0)
                        {
                            if (!Tags.TagExists(tagsInput[0]))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("The specified tag \"" + tagsInput[0] + "\" is not valid.");
                                Console.ResetColor();
                                allTagsValid = false;
                                continue;
                            }
                            else
                                tags.Add(tagsInput[0]);
                        }

                        // More than one (two) elements: add the second one too.
                        if (tagsInput.Length > 1)
                        {
                            if (!Tags.TagExists(tagsInput[1]))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("The specified tag \"" + tagsInput[1] + "\" is not valid.");
                                Console.ResetColor();
                                allTagsValid = false;
                                continue;
                            }
                            else
                                tags.Add(tagsInput[1]);
                        }

                        if (tagsInput.Length > 2)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("More than two tags specified. Only the first two is saved.");
                            Console.ResetColor();
                        }
                    }
                }
            }
            else
            {
                if (tagsInput[0] != String.Empty)
                {
                    // More than zero (one or two) elements: add the first one.
                    if (tagsInput.Length > 0)
                    {
                        if (!Tags.TagExists(tagsInput[0]))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The specified tag \"" + tagsInput[0] + "\" is not valid.");
                            Console.ResetColor();
                            return;
                        }
                        else
                            tags.Add(tagsInput[0]);
                    }

                    // More than one (two) elements: add the second one too.
                    if (tagsInput.Length > 1)
                    {
                        if (!Tags.TagExists(tagsInput[1]))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The specified tag \"" + tagsInput[1] + "\" is not valid.");
                            Console.ResetColor();
                            return;
                        }
                        else
                            tags.Add(tagsInput[1]);
                    }

                    if (tagsInput.Length > 2)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("More than two tags specified. Only the first two is saved.");
                        Console.ResetColor();
                    }
                }
            }

            addon.Tags = tags;
        }
    }
}