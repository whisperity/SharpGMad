using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGMad
{
    /// <summary>
    /// Represents a watcher declaration for an exported file.
    /// </summary>
    class FileWatch
    {
        /// <summary>
        /// Gets or sets the path of the file on the filesystem.
        /// </summary>
        public string FilePath;
        /// <summary>
        /// Gets or sets the path of the file in the loaded addon.
        /// </summary>
        public string ContentPath;
        /// <summary>
        /// Gets or sets whether the file is modified externally.
        /// </summary>
        public bool Modified;
        /// <summary>
        /// The integrated System.IO.FileSystemWatcher object.
        /// </summary>
        public FileSystemWatcher Watcher;
    }

    /// <summary>
    /// Provides methods and properties of handling realtime access in the commandline.
    /// </summary>
    static class Realtime
    {
        /// <summary>
        /// The current open addon.
        /// </summary>
        static Addon addon;
        /// <summary>
        /// The System.IO.FileStream connection to the current open addon on the disk.
        /// </summary>
        static FileStream addonFS;
        /// <summary>
        /// Gets or sets the full path of the current open addon.
        /// </summary>
        static string filePath;
        /// <summary>
        /// Contains the command-line prefix printed before each command reading.
        /// </summary>
        static string CommandlinePrefix = "SharpGMad>";
        /// <summary>
        /// Contains a list of externally exported watched files.
        /// </summary>
        static List<FileWatch> watchedFiles = new List<FileWatch>();
        /// <summary>
        /// Gets or sets whether the current addon is modified.
        /// </summary>
        static bool modified;
        /// <summary>
        /// Gets or sets whether the current addon has exported files which are pullable.
        /// </summary>
        static bool pullable;

        /// <summary>
        /// The main method and entry point for command-line operation.
        /// </summary>
        public static int Main(string[] args)
        {
            // There might be a parameter specified.
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

            // Autoload addon from command line arguments
            if (strFile != String.Empty && strFile != null)
                LoadAddon(strFile);

            while (true)
            {
                Console.Write(CommandlinePrefix + " ");
                string input = Console.ReadLine();
                string[] command = input.Split(' ');

                switch (command[0].ToLowerInvariant())
                {
                    case "new":
                        try
                        {
                            NewAddon(command[1]);
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
                            LoadAddon(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "add":
                        try
                        {
                            AddFile(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The file path was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "addfolder":
                        try
                        {
                            AddFolder(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The folder was not specified.");
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
                            RemoveFile(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "extract":
                        string extractPath = String.Empty;
                        try
                        {
                            extractPath = command[2];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Noop.
                        }

                        try
                        {
                            ExtractFile(command[1], extractPath);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "mget":
                        string folder;
                        try
                        {
                            folder = command[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The folder was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        if (!Directory.Exists(folder))
                        {
                            try
                            {
                                Directory.CreateDirectory(folder);
                            }
                            catch (IOException)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("There was a problem creating the output directory.");
                                Console.ResetColor();
                                break;
                            }
                        }

                        if ( command.Length < 3 )
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        string[] eparam = new string[command.Length - 2];
                        try
                        {
                            for (int i = 2; i < command.Length; i++)
                            {
                                eparam[i - 2] = command[i];
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Noop.
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There was an error parsing the filelist.");
                            Console.ResetColor();
                            break;
                        }

                        foreach (string path in eparam)
                        {
                            string outpath = folder + Path.DirectorySeparatorChar + Path.GetFileName(path);

                            ExtractFile(path, outpath);
                        }

                        break;
                    case "export":
                        string exportPath = String.Empty;
                        try
                        {
                            exportPath = command[2];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Noop.
                        }

                        try
                        {
                            ExportFile(command[1], exportPath);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (watchedFiles.Count == 0)
                            {
                                Console.WriteLine("No files are exported.");
                            }
                            else
                            {

                                Console.WriteLine(watchedFiles.Count + " files currently exported:");
                                int i = 0;
                                foreach (FileWatch watch in watchedFiles)
                                {
                                    Console.WriteLine(++i + 
                                        ((watch.Modified) ? "* " : " ") +
                                        watch.ContentPath + " at " + watch.FilePath);
                                }
                            }
                        }

                        break;
                    case "pull":
                        try
                        {
                            PullFile(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (watchedFiles.Count == 0)
                            {
                                Console.WriteLine("No files are exported.");
                            }
                            else
                            {

                                Console.WriteLine("Pulling " + watchedFiles.Count + " files:");
                                foreach (FileWatch watch in watchedFiles)
                                {
                                    PullFile(watch.ContentPath);
                                }
                            }
                        }

                        // Get whether there are files still pullable.
                        bool stillPullables = watchedFiles.Any(fw => fw.Modified == true);
                        SetPullable(stillPullables);

                        break;
                    case "drop":
                        try
                        {
                            DropExport(command[1]);
                        }
                        catch(IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "get":
                        string parameter;
                        try
                        {
                            parameter = command[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The parameter was not specified.");
                            Console.ResetColor();
                            Console.WriteLine("The valid paramteres are: author description tags title type");
                            break;
                        }

                        switch (parameter)
                        {
                            case "author":
                                Console.WriteLine(addon.Author);
                                break;
                            case "description":
                                Console.WriteLine(addon.Description);
                                break;
                            case "tags":
                                Console.WriteLine(String.Join(" ", addon.Tags.ToArray()));
                                break;
                            case "title":
                                Console.WriteLine(addon.Title);
                                break;
                            case "type":
                                Console.WriteLine(addon.Type);
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("The specified parameter is not valid.");
                                Console.ResetColor();
                                Console.WriteLine("The valid paramteres are: author description tags title type");
                                break;
                        }

                        break;
                    case "set":
                        string Sparameter;
                        try
                        {
                            Sparameter = command[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The parameter was not specified.");
                            Console.ResetColor();
                            Console.WriteLine("The valid paramteres are: author description tags title type");
                            break;
                        }

                        string value;
                        try
                        {
                            string[] param = new string[command.Length - 2];
                            for (int i = 2; i < command.Length; i++)
                            {
                                param[i - 2] = command[i];
                            }

                            value = String.Join(" ", param);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Noop.
                            value = String.Empty;
                        }

                        switch (Sparameter)
                        {
                            case "author":
                                SetAuthor(addon, value);
                                break;
                            case "description":
                                SetDescription(addon, value);
                                break;
                            case "tags":
                                SetTags(addon, value.Split(' '));
                                break;
                            case "title":
                                SetTitle(addon, value);
                                break;
                            case "type":
                                SetType(addon, value);
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("The specified parameter is not valid.");
                                Console.ResetColor();
                                Console.WriteLine("The valid paramteres are: author description tags title type");
                                break;
                        }

                        break;
                    case "close":
                        CloseAddon();
                        break;
                    case "push":
                        Push();
                        break;
                    case "path":
                        FullPath();
                        break;
                    case "pwd":
                        Console.WriteLine(Directory.GetCurrentDirectory());
                        break;
                    case "cd":
                        try
                        {
                            Directory.SetCurrentDirectory(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The folder was not specified.");
                            Console.ResetColor();
                            break;
                        }
                        catch (IOException e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There was a problem switching to that folder.");
                            Console.ResetColor();
                            Console.WriteLine(e.Message);
                            break;
                        }
                        break;
                    case "ls":
                        try
                        {
                            IEnumerable<string> files = Directory.EnumerateFileSystemEntries(Directory.GetCurrentDirectory(),
                                "*", SearchOption.TopDirectoryOnly);

                            if (files.Count() == 0)
                            {
                                Console.WriteLine("0 files.");
                                break;
                            }
                            else
                                Console.WriteLine(files.Count() + " files:");

                            foreach (string f in files)
                            {
                                FileSystemInfo fi;
                                fi = new FileInfo(f);

                                try
                                {
                                    Console.WriteLine(
                                        String.Format("{0,10} {1,20} {2,30}", ((int)((FileInfo)fi).Length).HumanReadableSize(),
                                        fi.LastWriteTime.ToString(), fi.Name)
                                    );
                                }
                                catch (FileNotFoundException)
                                {
                                    // Noop. The entry is a folder.
                                    fi = new DirectoryInfo(f);
                                    Console.WriteLine(
                                        String.Format("{0,10} {1,20} {2,30}", "[DIR]",
                                        fi.LastWriteTime.ToString(), fi.Name)
                                    );
                                    continue;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There was a problem listing the files.");
                            Console.ResetColor();
                            Console.WriteLine(e.Message);
                            break;
                        }

                        break;
                    case "gui":
                        if (addon == null)
                        {
                            System.Windows.Forms.Application.Run(new Main(new string[] { }));
                        }
                        else if (addon is Addon)
                        {
                            if (modified)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Your addon is modified.");
                                Console.ResetColor();
                                Console.WriteLine("The GUI cannot be opened until the addon is saved.");
                                break;
                            }
                            else
                            {
                                Console.WriteLine("The addon will close in the console and will be reopened by the GUI.");
                                // Save the addon path and close it in the console.
                                string addonpath = Realtime.filePath;
                                CloseAddon();

                                // Open the GUI with the path. The addon will automatically reload.
                                System.Windows.Forms.Application.Run(new Main(new string[] { addonpath }));
                                
                                // This thread will hang until the GUI is closed.
                                Console.WriteLine("GUI was closed. Reopening the addon...");
                                LoadAddon(addonpath);
                            }
                        }
                        break;
                    case "?":
                    case "help":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available commands:");
                        Console.ResetColor();

                        if (addon == null)
                        {
                            Console.WriteLine("load <filename>            Loads <filename> addon into the memory");
                            Console.WriteLine("new <filename>             Create a new, empty addon named <filename>");
                        }

                        if (addon is Addon)
                        {
                            Console.WriteLine("add <filename>             Adds <filename> to the archive");
                            Console.WriteLine("addfolder <folder>         Adds all files from <folder> to the archive");
                            Console.WriteLine("list                       Lists the files in the memory");
                            Console.WriteLine("remove <filename>          Removes <filename> from the archive");
                            Console.WriteLine("extract <filename> [path]  Extract <filename> (to [path] if specified)");
                            Console.WriteLine("mget <folder> <f1> [f2...] Extract all specified files to <folder>");
                            Console.WriteLine("export                     View the list of exported files");
                            Console.WriteLine("export <filename> [path]   Export <filename> for editing (to [path] if specified)");
                            Console.WriteLine("pull                       Pull changes from all exported files");
                            Console.WriteLine("pull <filename>            Pull the changes of exported <filename>");
                            Console.WriteLine("drop <filename>            Drops the export for <filename>");
                            Console.WriteLine("get <parameter>            Prints the value of metadata <parameter>");
                            Console.WriteLine("set <parameter> [value]    Sets metadata <parameter> to the specified [value]");
                            Console.WriteLine("push                       Writes the changes to the disk");
                            Console.WriteLine("close                      Closes the addon (dropping all changes)");
                            Console.WriteLine("path                       Prints the full path of the current addon.");
                        }

                        Console.WriteLine("pwd                        Prints SharpGMad's current working directory");
                        Console.WriteLine("cd <folder>                Changes the current working directory to <folder>");
                        Console.WriteLine("ls                         List all files in the current directory");

                        if (addon == null | (addon is Addon && !modified))
                        {
                            Console.WriteLine("gui                        Load the GUI");
                        }

                        Console.WriteLine("help                       Show the list of available commands");

                        if (addon == null)
                        {
                            Console.WriteLine("exit                       Exits");
                        }

                        if (addon is Addon)
                        {
                            if (modified)
                            {
                                Console.Write("The addon has been ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("modified");
                                Console.ResetColor();
                                Console.WriteLine(". Execute `push` to save changes to the disk.");
                            }

                            if (pullable)
                            {
                                Console.Write("There are exported files which have been changed. ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("These changes are pullable.");
                                Console.ResetColor();
                                Console.WriteLine("`export` lists all exports or type `pull` to pull the changes.");
                            }
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

        /// <summary>
        /// Creates a new addon.
        /// </summary>
        /// <param name="filename">The filename where the addon should be saved to.</param>
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

            SetTitle(addon);
            SetDescription(addon);
            //SetAuthor(addon); // Author name setting is not yet implemented.
            SetType(addon);
            SetTags(addon);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Successfully set up initial addon.");
            Console.ResetColor();

            // Write initial content
            filePath = Path.GetFullPath(filename);
            try
            {
                addonFS = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
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
            
            Writer.Create(addon, out ms);

            addonFS.Seek(0, SeekOrigin.Begin);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(addonFS);
            
            addonFS.Flush();
            ms.Dispose();

            SetModified(false);
            SetPullable(false);

            CommandlinePrefix = Path.GetFileName(filename) + ">";
            
        }

        /// <summary>
        /// Sets the title of an addon.
        /// </summary>
        /// <param name="addon">The addon which is to be modified</param>
        /// <param name="title">Optional. The new title the addon should have.</param>
        private static void SetTitle(Addon addon, string title = null)
        {
            if (title == String.Empty || title == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Title? ");
                Console.ResetColor();
                title = Console.ReadLine();
            }

            addon.Title = title;
        }

        /// <summary>
        /// Sets the description of an addon.
        /// </summary>
        /// <param name="addon">The addon which is to be modified</param>
        /// <param name="description">Optional. The new description the addon should have.</param>
        private static void SetDescription(Addon addon, string description = null)
        {
            if (description == String.Empty || description == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Short description? ");
                Console.ResetColor();
                description = Console.ReadLine();
            }

            addon.Description = description;
            SetModified(true);
        }

        /// <summary>
        /// Sets the author of an addon.
        /// </summary>
        /// <param name="addon">The addon which is to be modified</param>
        /// <param name="author">Optional. The new author the addon should have.</param>
        private static void SetAuthor(Addon addon, string author = null)
        {
            if (author == String.Empty || author == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Author? ");
                Console.ResetColor();
                Console.WriteLine("This currently has no effect as the addon is always written with \"Author Name\".");
                //author = Console.ReadLine();
                author = "Author Name";
            }

            addon.Author = author;
            SetModified(true);
        }

        /// <summary>
        /// Sets the type of an addon.
        /// </summary>
        /// <param name="addon">The addon which is to be modified</param>
        /// <param name="type">Optional. The new type the addon should have.</param>
        public static void SetType(Addon addon, string type = null)
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
            SetModified(true);
        }

        /// <summary>
        /// Sets the tags of an addon.
        /// </summary>
        /// <param name="addon">The addon which is to be modified</param>
        /// <param name="tagsInput">Optional. The new tags the addon should have.</param>
        public static void SetTags(Addon addon, string[] tagsInput = null)
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
            SetModified(true);
        }

        /// <summary>
        /// Loads an addon from the filesystem.
        /// </summary>
        /// <param name="filename">The path of the addon to load.</param>
        static void LoadAddon(string filename)
        {
            if (addon is Addon)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An addon is already open. Please close it first.");
                Console.ResetColor();
                return;
            }

            foreach (FileWatch watch in watchedFiles)
            {
                watch.Watcher.Dispose();
            }
            watchedFiles.Clear();

            Console.WriteLine("Loading file...");
            try
            {
                addonFS = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
                addon = new Addon(new Reader(addonFS));
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

            foreach (ContentFile f in addon.Files)
                Console.WriteLine("\t" + f.Path + " [" + ((int)f.Size).HumanReadableSize() + "]");

            filePath = Path.GetFullPath(filename);
            CommandlinePrefix = Path.GetFileName(filename) + ">";
            SetModified(false);
            SetPullable(false);
        }

        /// <summary>
        /// Adds a file to the open addon.
        /// </summary>
        /// <param name="filename">The path of the file to be added.</param>
        static void AddFile(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            byte[] bytes;
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, (int)fs.Length);
                }
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cannot add file. There was an error reading it.");
                Console.ResetColor();
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine(filename + " as");
            Console.WriteLine("\t" + Whitelist.GetMatchingString(filename));
            try
            {
                addon.AddFile(Whitelist.GetMatchingString(filename), bytes);
                SetModified(true);
            }
            catch (IgnoredException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t\t[Ignored]");
                Console.ResetColor();
                return;
            }
            catch (WhitelistException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t\t[Not allowed by whitelist]");
                Console.ResetColor();
                return;
            }
            catch (ArgumentException e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\t\t[A file like this has already been added. Remove it first.]");
                Console.ResetColor();
                return;
            }
        }

        /// <summary>
        /// Adds all files from a specified folder to the addon.
        /// </summary>
        /// <param name="folder">The folder containing the files to be added.</param>
        static void AddFolder(string folder)
        {
            foreach (string f in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                string file = f;
                file = file.Replace('\\', '/');

                AddFile(file);
            }
        }

        /// <summary>
        /// Lists the files currently added to the addon.
        /// </summary>
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

        /// <summary>
        /// Removes a file from the addon.
        /// </summary>
        /// <param name="filename">The path of the file to be removed.</param>
        static void RemoveFile(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            try
            {
                addon.RemoveFile(filename);
                SetModified(true);
            }
            catch (FileNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file was not found in the archive.");
                Console.ResetColor();
                return;
            }
        }

        /// <summary>
        /// Extract a file from the addon to a specified path on the local file system.
        /// Unlike ExportFile(), this does not set up a watch.
        /// </summary>
        /// <param name="filename">The path of the file in the addon to be exported.</param>
        /// <param name="extractPath">Optional. The path on the local file system where the file should be saved.
        /// If omitted, the file will be exported to the current working directory.</param>
        static void ExtractFile(string filename, string extractPath = null)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            IEnumerable<ContentFile> file = addon.Files.Where(f => f.Path == filename);

            if (file.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist in the archive.");
                Console.ResetColor();
                return;
            }

            if (extractPath == null || extractPath == String.Empty)
            {
                extractPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(filename);
            }

            if (File.Exists(extractPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A file at " + extractPath + " already exists. Aborting extraction!");
                Console.ResetColor();
                return;
            }

            FileStream extract;
            try
            {
                extract = new FileStream(extractPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem exporting the file.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                return;
            }
            extract.SetLength(0); // Truncate the file.
            extract.Write(file.First().Content, 0, (int)file.First().Size);
            extract.Flush();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Written " + ((int)extract.Length).HumanReadableSize() + " to " + extractPath + ".");
            Console.ResetColor();

            extract.Dispose();
        }

        /// <summary>
        /// Exports a file from the addon to a specified path on the local file system and sets up a watch.
        /// </summary>
        /// <param name="filename">The path of the file in the addon to be exported.</param>
        /// <param name="exportPath">Optional. The path on the local file system where the export should be saved.
        /// If omitted, the file will be exported to the current working directory.</param>
        static void ExportFile(string filename, string exportPath = null)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            IEnumerable<FileWatch> isExported = watchedFiles.Where(f => f.ContentPath == filename);
            if (isExported.Count() != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This file is already exported!");
                Console.ResetColor();
                return;
            }

            IEnumerable<ContentFile> file = addon.Files.Where(f => f.Path == filename);

            if (file.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist in the archive.");
                Console.ResetColor();
                return;
            }

            if (exportPath == null || exportPath == String.Empty)
            {
                exportPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(filename);
            }
            else
            {
                exportPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Path.GetFileName(exportPath);
            }

            if (File.Exists(exportPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A file at " + exportPath + " already exists. Aborting export!");
                Console.ResetColor();
                return;
            }

            FileStream export;
            try
            {
                export = new FileStream(exportPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem exporting the file.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                return;
            }
            export.SetLength(0); // Truncate the file.
            export.Write(file.First().Content, 0, (int)file.First().Size);
            export.Flush();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Written " + ((int)export.Length).HumanReadableSize() + " to " + exportPath + ".");
            Console.ResetColor();

            export.Dispose();

            // Set up a watcher
            FileSystemWatcher fsw = new FileSystemWatcher(Path.GetDirectoryName(exportPath), Path.GetFileName(exportPath));
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.EnableRaisingEvents = true;

            FileWatch watch = new FileWatch();
            watch.FilePath = exportPath;
            watch.ContentPath = filename;
            watch.Watcher = fsw;

            watchedFiles.Add(watch);
        }

        /// <summary>
        /// The event gets fired whenever a watched exported file gets modified externally.
        /// This event administers the changed state for the application.
        /// </summary>
        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.Name + " changed!");
            
            IEnumerable<FileWatch> search = watchedFiles.Where(f => f.FilePath == e.FullPath);

            if (search.Count() != 0)
            {
                //Console.WriteLine("Administering changed state.");

                IEnumerable<ContentFile> content = addon.Files.Where(f => f.Path == search.First().ContentPath);

                if (content.Count() == 1)
                {
                    search.First().Modified = true;
                    SetPullable(true);
                }
                else
                {
                    //Console.WriteLine("The linked content entry was not found.");
                    //Console.WriteLine("Disposing watch object.");
                    watchedFiles.Remove(search.First());
                    ((FileSystemWatcher)sender).Dispose();
                }
            }
            else
            {
                //Console.WriteLine("The file was not watched.");
                //Console.WriteLine("Disposing watch object.");
                watchedFiles.Remove(search.First());
                ((FileSystemWatcher)sender).Dispose();
            }
        }

        /// <summary>
        /// Removes a file export watcher and optionally deletes the exported file from the local file system.
        /// </summary>
        /// <param name="filename">The path of the file within the addon to be dropped.
        /// The exported path is known internally.</param>
        static void DropExport(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            IEnumerable<FileWatch> search = watchedFiles.Where(f => f.ContentPath == filename);

            if (search.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file is not in an exported state.");
                Console.ResetColor();
                return;
            }

            IEnumerable<ContentFile> content = addon.Files.Where(f => f.Path == search.First().ContentPath);
            if (content.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to find representing file in addon.");
                Console.ResetColor();
                Console.WriteLine("The watch is corrupted. Disposing.");
                search.First().Watcher.Dispose();
                return;
            }

            search.First().Watcher.Dispose();
            Console.WriteLine("Export dropped.");

            Console.Write("Delete the exported file too? (Y/N) ");
            string response = Console.ReadLine();
            if (response.ToUpperInvariant() == "Y")
            {
                try
                {

                    File.Delete(search.First().FilePath);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Was unable to delete the file.");
                    Console.ResetColor();
                    Console.WriteLine(e.Message);
                }
            }

            watchedFiles.Remove(search.First());
        }

        /// <summary>
        /// Pulls the changes of the specified file from its exported version.
        /// </summary>
        /// <param name="filename">The internal path of the file changes should be pulled into.
        /// The exported path is known automatically.</param>
        static void PullFile(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            IEnumerable<FileWatch> search = watchedFiles.Where(f => f.ContentPath == filename);

            if (search.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file is not in an exported state.");
                Console.ResetColor();
                return;
            }

            if (search.First().Modified == false)
            {
                Console.WriteLine("The file is not modified.");
                return;
            }

            IEnumerable<ContentFile> content = addon.Files.Where(f => f.Path == search.First().ContentPath);
            if (content.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to find representing file in addon.");
                Console.ResetColor();
                Console.WriteLine("The watch is corrupted. Disposing.");
                search.First().Watcher.Dispose();
                return;
            }

            Console.WriteLine(((int)content.First().Size).HumanReadableSize() + " in memory [CRC: " + content.First().CRC +
                "]");

            FileStream fs;
            try
            {
                fs = new FileStream(search.First().FilePath, FileMode.Open, FileAccess.Read);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to open exported file on disk.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine(((int)fs.Length).HumanReadableSize() + " exported on disk.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Pulling in changes...");
            Console.ResetColor();

            // Load and write the changes to memory
            byte[] contBytes = new byte[fs.Length];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(contBytes, 0, (int)fs.Length);
            content.First().Content = contBytes;

            fs.Dispose();

            Console.WriteLine("Pulled the changes. [New CRC: " +
                content.First().CRC + "]");

            // Consider the file unmodified
            search.First().Modified = false;

            // But the addon itself has been modified
            SetModified(true);
        }

        /// <summary>
        /// Saves the changes of the addon to the disk.
        /// </summary>
        static void Push()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            addon.Sort();

            MemoryStream ms;
            
            Writer.Create(addon, out ms);

            addonFS.Seek(0, SeekOrigin.Begin);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(addonFS);

            addonFS.Flush();
            ms.Dispose();

            SetModified(false);
            Console.WriteLine("Successfully saved the addon.");
        }

        /// <summary>
        /// Closes the currently open addon connection.
        /// </summary>
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
            addonFS.Dispose();
            SetModified(false);
            SetPullable(false);

            foreach (FileWatch watch in watchedFiles)
            {
                watch.Watcher.Dispose();
            }
            watchedFiles.Clear();
        }

        /// <summary>
        /// Prints the full path of the currently open addon to the console.
        /// </summary>
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

        /// <summary>
        /// Sets the currently open addon's modified state.
        /// </summary>
        static void SetModified(bool modified)
        {
            if (modified)
            {
                if (addon != null)
                    CommandlinePrefix = Path.GetFileName(Realtime.filePath) + "*>";
            }
            else
            {
                if (addon != null)
                    CommandlinePrefix = Path.GetFileName(Realtime.filePath) + ">";
            }

            Realtime.modified = modified;
        }

        /// <summary>
        /// Sets whether there are exported files which can be pulled.
        /// </summary>
        static void SetPullable(bool pullable)
        {
            SetModified(Realtime.modified); // Set the modified state so it resets the shell prefix

            if (pullable)
            {
                CommandlinePrefix = CommandlinePrefix.TrimEnd('>');
                CommandlinePrefix += "#>";
            }

            Realtime.pullable = pullable;
        }
    }
}