using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    class CreateAddon
    {
        static public bool VerifyFiles(ref List<string> files, bool warnInvalid)
        {
            bool bOk = true;

            //
            // Bail out if there's no files
            //
            if (files.Count == 0)
            {
                Console.WriteLine("No files found, can't continue!");
                bOk = false;
            }

            List<string> old_files = new List<string>(files);
            files.Clear();
            //
            // Print each found file, check they're ok
            //
            foreach (string file in old_files)
            {
                Console.WriteLine("\t" + file);

                //
                // Check the file against the whitelist
                // Lowercase the name (addon filesystem is case insentive)
                //
                if (Addon.Whitelist.Check(file.ToLowerInvariant()))
                    files.Add(file);
                else
                {
                    Output.Warning("\t\t[Not allowed by whitelist]");
                    if (!warnInvalid)
                        bOk = false;
                }

                //
                // Warn that we're gonna lowercase the filename
                if (file.ToLowerInvariant() != file)
                {
                    Output.Warning("\t\t[Filename contains capital letters]");
                }
            }
            return bOk;
        }
    }
}
