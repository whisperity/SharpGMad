using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    partial class Program
    {
        static Addon addon;
        static string filePath;
        static string CommandlinePrefix = "SharpGMad>";

        static int RealtimeCommandline(string strFile)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Realtime command-line activated.");
            Console.ResetColor();

            // Autoload addon from command line arguments
            if (strFile != String.Empty && strFile != null)
                LoadAddon(strFile);

            while (true)
            {
                Console.Write(CommandlinePrefix + " ");
                string input = Console.ReadLine();
                string[] args = input.Split(' ');

                switch (args[0])
                {
                    case "new":
                        try
                        {
                            NewAddon(args[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "load":
                        try
                        {
                            LoadAddon(args[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "list":
                        ListFiles();
                        break;
                    case "remove":
                        try
                        {
                            RemoveFile(args[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "abort":
                        CloseAddon();
                        break;
                    case "push":
                        Push();
                        break;
                    case "close":
                        Push();
                        CloseAddon();
                        break;
                    case "path":
                        FullPath();
                        break;
                    case "?":
                    case "help":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available commands:");
                        Console.ResetColor();

                        if (addon == null)
                        {
                            Console.WriteLine("load <filename>                Loads <filename> addon into the memory");
                            Console.WriteLine("new <filename>                 Create a new, empty addon named <filename>");
                        }

                        if (addon is Addon)
                        {
                            Console.WriteLine("list                           Lists the files in the memory");
                            Console.WriteLine("remove <filename>              Removes <filename> from the archive");
                            Console.WriteLine("push                           Writes the addon to the disk");
                            Console.WriteLine("close                          Writes addon and closes it");
                            Console.WriteLine("abort                          Unloads the addon from memory, dropping all changes");
                            Console.WriteLine("path                           Prints the full path of the current addon.");
                        }

                        Console.WriteLine("help                               Show the list of available commands");

                        if (addon == null)
                        {
                            Console.WriteLine("exit                               Exits");
                        }

                        break;
                    case "exit":
                        if (addon is Addon)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Cannot exit. An addon is open!");
                            Console.ResetColor();
                            break;
                        }

                        return 0;
                        //break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown operation.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        static void NewAddon(string filename)
        {
            if (addon is Addon)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An addon is already open. Please close it first.");
                Console.ResetColor();
                return;
            }

            if (File.Exists(filename))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("This file already exists. You might want to load it instead.");
                Console.ResetColor();
                return;
            }

            //
            // Make sure OutFile ends in .gma
            //
            filename = Path.GetFileNameWithoutExtension(filename);
            filename += ".gma";

            Console.WriteLine("Creating new file...");

            CommandlinePrefix = filename + " (setup)>";
            addon = new Addon();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Title? ");
            Console.ResetColor();
            string title = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Short description? ");
            Console.ResetColor();
            string desc = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Author? ");
            Console.ResetColor();
            Console.WriteLine("This currently has no effect as the addon is always written with \"Author Name\".");
            //string author = Console.ReadLine();
            string author = "Author Name";

            string type = String.Empty;
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

            List<string> tags = new List<string>(2);
            bool allTagsValid = false;
            while (!allTagsValid)
            {
                tags.Clear();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Tags? ");
                Console.ResetColor();
                Console.Write("Please choose ZERO, ONE or TWO from the following: ");
                Console.WriteLine(String.Join(" ", Tags.Misc));

                string[] tagsInput = Console.ReadLine().Split(' ');

                allTagsValid = true;

                // More than zero (one or two) elements: add the first one.
                if (tagsInput.Count() > 0)
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
                if (tagsInput.Count() > 1)
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
            }

            addon.Title = title;
            addon.Description = desc;
            addon.Author = author;
            addon.Type = type;
            addon.Tags = tags;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Successfully set up initial addon.");
            Console.ResetColor();

            // Write initial content
            filePath = filename;
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem opening the file.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                CloseAddon();
                return;
            }

            MemoryStream ms;
            StreamDiffer sd = new StreamDiffer(fileStream);
            Writer.Create(addon, out ms);
            sd.Write(ms);
            sd.Push();

            fileStream.Close();

            CommandlinePrefix = filename + ">";
        }

        static void LoadAddon(string filename)
        {
            if (addon is Addon)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An addon is already open. Please close it first.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Loading file...");

            addon = new Addon(new Reader(filename));

            foreach (ContentFile f in addon.Files)
                Console.WriteLine("\t" + f.Path + " [" + ((int)f.Size).HumanReadableSize() + "]");

            filePath = filename;
            CommandlinePrefix = filename + ">";
        }

        static void ListFiles()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine(addon.Files.Count + " files in archive:");
            foreach (ContentFile file in addon.Files)
            {
                Console.WriteLine(file.Path + " (" + ((int)file.Size).HumanReadableSize() + ")");
            }
        }

        static void RemoveFile(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            addon.RemoveFile(filename);
        }

        static void Push()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem opening the file.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                return;
            }

            MemoryStream ms;
            StreamDiffer sd = new StreamDiffer(fileStream);
            Writer.Create(addon, out ms);
            sd.Write(ms);
            sd.Push();

            fileStream.Close();
        }

        static void CloseAddon()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            addon = null;
            CommandlinePrefix = "SharpGMad>";
        }

        static void FullPath()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine(Path.GetFullPath(filePath));
        }
    }
}