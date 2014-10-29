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
#if WINDOWS
        /// <summary>
        /// External method to find a pointer for an attached console window.
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow")]
        private static extern IntPtr _GetConsoleWindow();
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Clean up mess the previous executions might have left behind.
            ContentFile.DisposeExternals();

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
                if (args[0] == "create" || args[0] == "extract")
                    // Load the legacy (gmad.exe) interface
                    return Legacy.Main(args);
                else if (args[0] == "realtime")
                    // Load the realtime command-line
                    return RealtimeCommandline.Main(args);
            }
            else
            {
#if WINDOWS
                IntPtr consoleHandle = _GetConsoleWindow();
                bool dontRestartMyself = consoleHandle == IntPtr.Zero;

#if DEBUG
                dontRestartMyself = dontRestartMyself || AppDomain.CurrentDomain.FriendlyName.Contains(".vshost");
#endif

                if (dontRestartMyself)
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
#endif

#if MONO
                Application.Run(new Main(args));
#endif
            }

            return 0;
        }

        /// <summary>
        /// Gets the version of the executing assembly with omitting the trailing zeros.
        /// </summary>
        static public string PrettyVersion
        {
            get
            {
                Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                int fieldCount = 0;

                // Increment the required fields until there is a value (this emits the trailing zeros)
                if (ver.Major != 0)
                    fieldCount = 1;
                if (ver.Minor != 0)
                    fieldCount = 2;
                if (ver.Build != 0)
                    fieldCount = 3;
                if (ver.Revision != 0)
                    fieldCount = 4;

                return "v" + ver.ToString(fieldCount);
            }
        }

    }
}