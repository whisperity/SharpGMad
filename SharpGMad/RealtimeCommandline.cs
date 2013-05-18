﻿using System;
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
                    case "load":
                        if (addon is Addon)
                        {
                            Output.Warning("An addon is already open. Please close it first.");
                            break;
                        }

                        try
                        {
                            LoadAddon(args[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine("The filename was not specified.");
                            break;
                        }

                        break;
                    case "list":
                        if ( addon == null )
                        {
                            Output.Warning("No addon is open.");
                            break;
                        }

                        ListFiles();
                        break;
                    case "remove":
                        if ( addon == null)
                        {
                            Output.Warning("No addon is open.");
                            break;
                        }

                        try
                        {
                            RemoveFile(args[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine("The filename was not specified.");
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
                    case "?":
                    case "help":
                        Console.WriteLine("Available commands:");
                        Console.WriteLine("load <filename>                    Loads <filename> addon into the memory");

                        if (addon is Addon)
                        {
                            Console.WriteLine("list                               Lists the files in the memory");
                            Console.WriteLine("remove <filename>                  Removes <filename> from the archive");
                            Console.WriteLine("push                               Writes the addon to the disk");
                            Console.WriteLine("close                              Writes addon and closes it");
                            Console.WriteLine("abort                              Unloads the addon from memory, dropping all changes");
                        }

                        Console.WriteLine("help                               Show the list of available commands");
                        Console.WriteLine("exit                               Exits");

                        break;
                    case "exit":
                        if (addon is Addon)
                        {
                            Output.Warning("Cannot exit. An addon is open!");
                            break;
                        }

                        return 0;
                        //break;
                    default:
                        Output.Warning("Unknown operation.");
                        break;
                }
            }
        }
        
        static void LoadAddon(string filename)
        {
            Console.WriteLine("Loading file...");

            addon = new Addon(new Reader(filename));
            filePath = filename;
            CommandlinePrefix = filename + ">";
        }

        static void ListFiles()
        {
            Console.WriteLine(addon.Files.Count + " files in archive:");
            foreach (ContentFile file in addon.Files)
            {
                Console.WriteLine(file.Path + " (" + Program.Memory((int)file.Size) + ")");
            }
        }

        static void RemoveFile(string filename)
        {
            addon.RemoveFile(filename);
        }

        static void Push()
        {
            if (addon == null)
            {
                Output.Warning("No addon is open.");
                return;
            }

            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception e)
            {
                Output.Warning("There was a problem opening the file.");
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
                Output.Warning("No addon is open.");
                return;
            }

            addon = null;
            CommandlinePrefix = "SharpGMad>";
        }
    }
}