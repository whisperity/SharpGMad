using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    partial class Program
    {
        static UpdatableAddon addon;
        static string CommandlinePrefix = "SharpGMad>";

        static int RealtimeCommandline()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Realtime command-line activated.");
            Console.ResetColor();

            while (true)
            {
                Console.Write(CommandlinePrefix + " ");
                string input = Console.ReadLine();
                string[] args = input.Split(' ');

                switch (args[0])
                {
                    case "load":
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
                        ListFiles();
                        break;
                    case "remove":
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
                    default:
                        Console.WriteLine("Unknown operation.");
                        break;
                }
            }
        }
        
        static void LoadAddon(string filename)
        {
            Console.WriteLine("Loading file...");

            addon = new UpdatableAddon();

            Reader r = new Reader(filename);

            /*Addon.Reader r = new Addon.Reader();
            if (!r.ReadFromFile(filename))
            {
                Console.WriteLine("There was a problem opening the file.");
                return;
            }

            if (!r.Parse())
            {
                Console.WriteLine("There was a problem parsin the file.");
                return;
            }*/
            
            addon.Author = r.Author;
            addon.Title = r.Name;
            addon.Description = r.Description;
            addon.Type = r.Type;
            addon.Tags = r.Tags;

            Console.WriteLine("Loaded addon " + addon.Title);
            Console.WriteLine("Loading files from GMA...");

            foreach (Addon.Format.FileEntry file in r.Index)
            {
                MemoryStream buffer = new MemoryStream();
                r.GetFile(file.iFileNumber, buffer);

                buffer.Seek(0, SeekOrigin.Begin);

                byte[] bytes = new byte[buffer.Length];
                buffer.Read(bytes, 0, (int)buffer.Length);

                addon.AddFile(file.strName, bytes);

                Console.WriteLine(file.strName + " loaded.");
            }

            addon.UpdateInternalStream();

            Console.WriteLine("Addon opened successfully.");

            FileStream filestream;
            try
            {
                filestream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was a problem opening the file.");
                Console.WriteLine(e.Message);
                return;
            }

            StreamDiffer sd = new StreamDiffer(filestream);
            sd.Write(addon.Buffer);
            sd.Push();
        }

        static void ListFiles()
        {
            Console.WriteLine(addon.Files.Count + " files in archive:");
            foreach (UpdatableAddon.ContentFile file in addon.Files)
            {
                Console.WriteLine(file.Path + " (" + Program.Memory((int)file.Size) + ")");
            }
        }

        static void RemoveFile(string filename)
        {
            addon.RemoveFile(filename);
        }
    }
}