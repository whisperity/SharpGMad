using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SharpGMad
{
    /// <summary>
    /// Contains basic methods for SharpGMad, like the entry point.
    /// </summary>
    class Program
    {
        /// <summary>
        /// External method to find a pointer for an attached console window.
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow")]
        private static extern IntPtr _GetConsoleWindow();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            /*Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Sharp Garry's Addon Creator 1.0");
            Console.ResetColor();*/

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // If there are parameters present, program starts as a CLI application.
            //
            // If there are no paremeters, the program is restarted in its own console
            // If there are no parameters and no console present, the GUI will start.
            // (This obviously means a small flickering of a console window (for the restart process)
            // but that's expendable for the fact that one compiled binary contains "both" faces.)

            if ((args != null && args.Length > 0) && ( args[0] == "create" || args[0] == "extract" || args[0] == "realtime"))
            {
                // This is needed because we support "drag and drop" GMA onto the executable
                // and if a D&D happens, the first parameter (args[0]) is a path.

                // There was a requirement for the console interface. Parse the parameters.
                if ( args[0] == "create" || args[0] == "extract" )
                    // Load the legacy (gmad.exe) interface
                    return Legacy.Main(args);
                else if ( args[0] == "realtime" )
                    // Load the realtime command-line
                    return Realtime.Main(args);

                //
                // Help
                //
                Console.WriteLine("Usage:");
                Console.WriteLine();
            }
            else
            {
                IntPtr consoleHandle = _GetConsoleWindow();

                if (consoleHandle == IntPtr.Zero || AppDomain.CurrentDomain.FriendlyName.Contains(".vshost"))
                    // There is no console window or this is a debug run.
                    // Start the main form

                    Application.Run(new Main(args));
                else
                {
                    // There is a console the program is running in.
                    // Restart itself without one.
                    // (This is why the little flicker happens.)
                    Process.Start(new ProcessStartInfo(Assembly.GetEntryAssembly().Location, String.Join(" ", args))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                    });
                }
            }

            return 0;
        }
    }
}