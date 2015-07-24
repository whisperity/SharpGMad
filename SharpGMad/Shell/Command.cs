using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGMad.Shell
{
    // TODO: support for accessibility
    [Flags]
    enum CommandAvailability : byte
    {
        Default = 0,

    }

    /// <summary>
    /// A command-line command
    /// </summary>
    class Command
    {
        /// <summary>
        /// The "name" of the command
        /// </summary>
        public string Verb { get; protected set; }
        /// <summary>
        /// Short description - usually one line telling what the command does
        /// </summary>
        public string Description;
        public string LongDescription;
        /// <summary>
        /// The list of arguments the command can have
        /// </summary>
        protected List<Argument> _Arguments;
        /// <summary>
        /// The list of arguments the command can have
        /// </summary>
        public List<Argument> Arguments
        {
            get
            {
                if (_Arguments != null)
                    return _Arguments.AsReadOnly().ToList();
                else
                    return null;
            }
        }
        /// <summary>
        /// The action to execute when the command is ran
        /// </summary>
        /// <summary>
        /// The action to execute when the command is ran
        /// </summary>
        protected Action<Command> Dispatch;

        public bool IsAlias { get; protected set; }
        public bool IsGroup { get; protected set; } // TODO: command groups - not yet implemented
        public bool IsOverload { get; protected set; }

        protected Command()
        {
            this._Arguments = new List<Argument>();
            this.IsAlias = false;
            this.IsOverload = false;
        }

        /// <summary>
        /// Create a new command
        /// </summary>
        /// <param name="verb">The verb of the command</param>
        /// <param name="dispatch">The code to run if the command is executed</param>
        public Command(string verb, Action<Command> dispatch)
            : this()
        {
            this.Verb = verb;
            this.Dispatch = dispatch;
        }

        /// <summary>
        /// Generate a short usage string (the list of arguments) for this command
        /// </summary>
        public virtual string UsageShort()
        {
            if (IsOverload)
                return ((CommandOverload)this).UsageShort();

            System.Text.StringBuilder sb = new System.Text.StringBuilder(this.Verb);

            if (this.Arguments != null)
                if (this.Arguments.Count > 0)
                {
                    sb.Append(' ');
                    foreach (Argument arg in this.Arguments)
                        sb.Append(
                            (arg.Mandatory ? "<" : "[")
                            + arg.Name
                            + (arg.MultiParams ? "..." : String.Empty)
                            + (arg.Mandatory ? ">" : "]")
                            + " "
                        );
                }

            return sb.ToString().TrimEnd(' ');
        }

        /// <summary>
        /// Generate a medium usage string (the list of arguments and short description) for this command
        /// </summary>
        /// <param name="longestDescriptionLength">The length of the longest description of the printed commands
        /// (when printing multiple commands), the output is aligned to a nice table of one-line rows if possible.</param>
        public virtual string UsageMedium(int longestDescriptionLength)
        {
            if (IsOverload)
                return ((CommandOverload)this).UsageMedium(longestDescriptionLength);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(this.UsageShort());
            if (this.Description.Length <= longestDescriptionLength)
            {
                sb.Append(new String(' ', Console.BufferWidth - this.UsageShort().Length - longestDescriptionLength - 1));
                sb.Append(this.Description);
            }
            else
            {
                sb.AppendLine();
                sb.Append("\t" + this.Description);
            }

            return sb.ToString();
        }

        public static int CalculateLongestFittingDescriptionLength(IEnumerable<Command> commands)
        {
            if (commands.Count() == 0)
                return Console.BufferWidth;

            // First, get the commands with the longest short usage string
            List<Command> cByArgshelpLength = commands
                .OrderByDescending(c => c.UsageShort().Length).ToList();
            //foreach (Command com in cByArgshelpLength)
            //{
            //    Console.WriteLine(com.UsageShort());
            //}

            int longestShort = cByArgshelpLength[0].UsageShort().Length;
            int maxDescLength = Console.BufferWidth - 1 - longestShort;

            // Find the longest description that could fit in one line with the longest UsageShort() output
            IEnumerable<Command> longestFittingDesc = commands
                .Where(c => !String.IsNullOrWhiteSpace(c.Description))
                .Where(c => c.Description.Length <= maxDescLength)
                .OrderByDescending(c => c.Description.Length);

            if (longestFittingDesc.Count() > 0)
                maxDescLength = longestFittingDesc.First().Description.Length - 1;


            if (longestFittingDesc.Count() > 0)
                return longestFittingDesc.First().Description.Length;
            else
                return Console.BufferWidth;
        }

        /// <summary>
        /// Add the given argument to the arguments of this command
        /// </summary>
        public void AddArgument(Argument arg)
        {
            if (this._Arguments.Count(a => a.Name == arg.Name) != 0)
                throw new ArgumentException("An argument with the name " + arg.Name + " already exists.");

            // The argument list should be that mandatory arguments are first
            // and only one MultiParams argument can exist, and it must be last
            if (this._Arguments.Count > 0)
            {
                if (arg.Mandatory && !this._Arguments[this._Arguments.Count - 1].Mandatory)
                    throw new ArgumentException("Mandatory arguments must precede all non-mandatory ones.");

                if (this._Arguments[this._Arguments.Count - 1].MultiParams)
                    throw new ArgumentException("The MultiParams argument of the command must be the last one" +
                        (arg.MultiParams ? ", and only one such argument can exist" : String.Empty) + ".");
            }

            this._Arguments.Add(arg);
        }

        /// <summary>
        /// Retrieve the argument having the given name from the argument list
        /// </summary>
        /// <param name="name">The name of the argument to get</param>
        /// <returns>An Argument object</returns>
        public Argument GetArgument(string name)
        {
            return this._Arguments.Where(arg => arg.Name == name).FirstOrDefault();
        }

        /// <summary>
        /// Parse the given argument string (args) and get the left-most value from it as 'result'.
        /// </summary>
        // Valid arguments are separated by spaces and, optionally, quotes
        private static bool GetArgumentFromLine(string args, out string result)
        {
            // To store the current argument
            result = String.Empty;

            if (args.Length == 0)
                return false;

            if (args[0] == '"')
            {
                int nextQuote = 1;
                bool nextQuoteFound = false;
                if (args.Length >= 2 && args[nextQuote] == '"')
                    nextQuoteFound = true;

                while (!nextQuoteFound)
                {
                    nextQuote = args.IndexOf('"', nextQuote + 1); // Search for the next quote mark
                    if (nextQuote != -1)
                    {
                        if (args[nextQuote - 1] != '\\') // If the next quote symbol is escaped, skip it
                            nextQuoteFound = true;
                    }
                    else
                        throw new ArgumentException("Malformed argument, the quoted block is not closed properly");
                }

                // The argument is the one between the quotes
                result = args.Substring(1, nextQuote - 1);
            }
            else if (args[0] == ' ')
            {
                // Skip if there are multiple SPACEs
                char ch = ' ';
                int c = 0;
                while (ch == ' ' && c < args.Length)
                    ch = args[++c];

                return GetArgumentFromLine(args.Substring(c), out result);
            }
            else
            {
                // Parse the argument as a single-word one
                // It's like being in a quote block, but now we are in a "space block"
                int nextSpace = 0;
                bool nextSpaceFound = false;

                while (!nextSpaceFound)
                {
                    nextSpace = args.IndexOf(' ', nextSpace + 1); // Search for the next quote mark
                    if (nextSpace != -1)
                    {
                        if (args[nextSpace - 1] != '\\') // If the next quote symbol is escaped, skip it
                            nextSpaceFound = true;
                    }
                    else
                    {
                        // If the next space was not found, it "is" at the end of the argument
                        nextSpace = args.Length;
                        nextSpaceFound = true;
                    }
                }

                // The argument is the one between the spaces
                result = args.Substring(0, nextSpace);
                if (result.IndexOf('"') != -1)
                    throw new ArgumentException("Malformed argument, found an unescaped quote sequence.");
            }

            return true;
        }

        public static string[] ParseArgumentLine(string argText)
        {
            List<string> argumentParts = new List<string>();
            while (!String.IsNullOrWhiteSpace(argText))
            {
                string stringArg = String.Empty;

                try
                {
                    if (!GetArgumentFromLine(argText, out stringArg))
                        // If we are unable to parse the argument, signal it and stop trying
                        throw new IndexOutOfRangeException("Command line finished unexpectedly");
                    else
                    {
                        // Consume the found argument from the list of arguments to parse
                        // Remove heading spaces
                        argText = argText.TrimStart(' ');
                        if (argText != stringArg)
                        {
                            int foundArgumentStart = argText.IndexOf(stringArg);
                            argText = argText.Substring(foundArgumentStart + stringArg.Length + 1);

                            // Consume a separator SPACE
                            if (argText.Length > 0 && argText[0] == ' ')
                                argText = argText.Substring(1);
                        }
                        else
                            argText = String.Empty;

                        stringArg = stringArg.Replace("\\\"", "\"").Replace("\\ ", " "); // Remove the escape sequence from the argument
                        argumentParts.Add(stringArg);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return argumentParts.ToArray();
        }

        /// <summary>
        /// Run the command with the given arguments
        /// </summary>
        /// <param name="argText">The full command-line (without the command's name) containing the possible argument values</param>
        public void Invoke(string argText)
        {
            if (this.Dispatch == null)
                return;

            // Handle the execution if the current command is not a "real" one
            if (this.IsAlias)
            {
                ((CommandAlias)this).Invoke(argText);
                return;
            }

            if (this.IsOverload)
            {
                ((CommandOverload)this).Invoke(argText);
                return;
            }

            // Bind all shell arguments to their values
            // This is done by consuming the argText variable containing all the shell arguments
            string[] args;
            try
            {
                args = ParseArgumentLine(argText);
            }
            catch (Exception e)
            {
                ConsoleExtensions.WriteColor("Unable to parse the given command.", ConsoleColor.Red);
                if (!String.IsNullOrWhiteSpace(e.Message))
                    Console.WriteLine(e.Message);
                return;
            }

            int i = 0;
            foreach (Argument arg in this._Arguments)
            {
                // If the argument is a "params" one, run it as much as we can.
                bool keepRunning = true;

                while (keepRunning)
                {
                    string stringArg = String.Empty;

                    try
                    {
                        stringArg = args[i++];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // We need to show an error message if the argument is mandatory and if it does not have a value
                        if (arg.Mandatory && !arg.HasValue)
                        {
                            ConsoleExtensions.WriteColor("missing argument " + arg.Name, ConsoleColor.Red);
                            Console.WriteLine("Usage: " + this.UsageShort());

                            return; // Don't parse further
                        }

                        // Don't parse further if there's no value to be found and it's a params argument
                        // This usually indicates that the arguments were all parsed and there's nothing left
                        if (arg.MultiParams)
                        {
                            keepRunning = false;
                            continue;
                        }
                    }

                    try
                    {
                        // Set the value for the argument to the found one
                        // (or add it to the array or arguments if arg.MultiParams is true)
                        arg.SetValue(stringArg);
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("invalid value for " + arg.Name);
                        Console.ResetColor();

                        if (String.IsNullOrWhiteSpace(e.Message) && e.InnerException != null
                            && !String.IsNullOrWhiteSpace(e.InnerException.Message))
                            Console.WriteLine(" " + e.InnerException.Message);
                        else
                            Console.WriteLine(" " + e.Message);

                        return;
                    }

                    // Only run the parser block once if the currently parsed is not a params argument
                    if (!arg.MultiParams)
                        keepRunning = false;
                }
            }

            // Call the associated method
            this.Dispatch(this);

            // Reset the arguments
            foreach (Argument arg in this._Arguments)
                arg.Reset();
        }
    }

    /// <summary>
    /// An alias for a real command
    /// </summary>
    class CommandAlias : Command
    {
        /// <summary>
        /// The CommandRegistry where the real command is registered
        /// </summary>
        private CommandRegistry ContextRegistry;
        /// <summary>
        /// The "name" of the real command
        /// </summary>
        private string RealVerb;

        /// <summary>
        /// Create an alias for realVerb named aliasVerb. realVerb is registered in "context"
        /// </summary>
        /// <param name="aliasedVerb">The alias to provide</param>
        /// <param name="realVerb">The command to alias</param>
        /// <param name="context">The command registry where the command named <see cref="realVerb" /> is found</param>
        public CommandAlias(string aliasedVerb, string realVerb, CommandRegistry context)
        {
            Command realCommand = context.Get(realVerb);

            base.Verb = aliasedVerb;
            this.RealVerb = realVerb;
            base.Description = null;
            base.IsAlias = true;
            base._Arguments = null;

            this.ContextRegistry = context;
            this.Dispatch = (self) =>
            {
                throw new InvalidOperationException("Error: The dispatch for an alias method should NEVER be called...");
            };
        }

        /// <summary>
        /// Run the aliased command with the given arguments
        /// </summary>
        /// <param name="argText">The full command-line (without the command's name) containing the possible argument values</param>
        public new void Invoke(string argText)
        {
            Command realCommand = this.ContextRegistry.Get(this.RealVerb);
            realCommand.Invoke(argText);
        }
    }

    /// <summary>
    /// A collection of commands that are overloaded: they have the same verb, but the number of arguments differ
    /// </summary>
    class CommandOverload : Command
    {
        private Dictionary<string, Command> Commands;

        /// <summary>
        /// Create a new overloaded command which can store different commands to execute under the same name
        /// </summary>
        /// <param name="verb">The name of the commands which will be called</param>
        public CommandOverload(string verb)
        {
            this.Commands = new Dictionary<string, Command>();
            base.Description = null;
            base.Verb = verb;
            base.IsOverload = true;
            base._Arguments = null;

            this.Dispatch = (self) =>
            {
                throw new InvalidOperationException("Error: The dispatch for an overloaded method should NEVER be called...");
            };
        }

        /// <summary>
        /// Add the given command to the overloads of this command
        /// </summary>
        /// <param name="uniqueName">A unique name to identify this particular overload</param>
        /// <param name="com">The command to add</param>
        public void Add(string uniqueName, Command com)
        {
            IEnumerable<KeyValuePair<string, Command>> check_exact =
                this.Commands.Where(c => c.Value.Arguments.Count == com.Arguments.Count);
            if (check_exact.Count() != 0)
                throw new ArgumentException("A command with the exact same argument count already exists as "
                    + check_exact.First().Key);

            // Check for mandatory-non mandatory shadowing
            /* Consider the following code:
             * void A(string a) { Console.WriteLine("1"); }
             * void A(string a, string b = null) { Console.WriteLine("2"); }
             *
             * Calling A("a"); will always print 1, both functions have the same number of "mandatory" arguments.
             */
            IEnumerable<KeyValuePair<string, Command>> check_mandatory =
                this.Commands.Where(c =>
                    c.Value.Arguments.Where(arg => arg.Mandatory).Count() ==
                    com.Arguments.Where(arg => arg.Mandatory).Count());
            if (check_mandatory.Count() != 0)
                throw new ArgumentException("A command with the exact same mandatory argument count already exists as "
                    + check_mandatory.First().Key);

            // Prevent issues if one command has the same number of arguments as the current to-be-added-one's mandatory count.
            // So command "a <a> [b]" and "a <a> <b> [c]" can't coexists (<a> is mandatory, [a] is optional argument.)
            // Because calling "a 1 2" is seemingly ambigous. (Such in C# automatically resolve to the first one, though.)
            IEnumerable<KeyValuePair<string, Command>> check_mandatory2 =
                this.Commands.Where(c =>
                    c.Value.Arguments.Count ==
                    com.Arguments.Where(arg => arg.Mandatory).Count());
            if (check_mandatory2.Count() != 0)
                throw new ArgumentException("A command with the exact same mandatory argument count already exists as "
                    + check_mandatory2.First().Key);

            // Check for params shadowing
            /* Consider the following code:
             * void A(string a) { Console.WriteLine("1"); }
             * void A(params string[] a) { Console.WriteLine("2"); }
             * void A(string a, params string[] b) { Console.WriteLine("3"); }
             *
             * Calling A("a"); will print 1
             * Calling A("a", "b"); will print 3
             * Calling A("a", "b", "c") will print 3... etc.
             */
            // The one overloaded command with MultiParams argument must have the highest total argument count
            // And only one overload can exist with a params argument (the terminal is not strongly typed...)
            IEnumerable<KeyValuePair<string, Command>> check_params =
                this.Commands.Where(c => c.Value.Arguments.Any(arg => arg.MultiParams));
            if (check_params.Count() != 0)
            {
                if (com.Arguments.Any(arg => arg.MultiParams))
                    throw new ArgumentException("Only one overload of " + this.Verb + " can exist with a MultiParams argument."
                        + "\n" + check_params.First().Key + " already satisfies this condition.");

                if (check_params.First().Value.Arguments.Count < com.Arguments.Count)
                    throw new ArgumentException("The MultiParams overload of " + this.Verb + " must be the one " +
                        "with the highest overload count. " + check_params.First().Key + " has " +
                        check_params.First().Value.Arguments.Count +
                        ", so adding one with " + com.Arguments.Count + " is disallowed.");
            }

            if (com.Verb != this.Verb)
                throw new ArgumentException("The overloaded command's name must be the same as the overload group's name.");

            this.Commands.Add(uniqueName, com);
        }

        public Dictionary<string, Command> GetCommands()
        {
            return new Dictionary<string, Command>(this.Commands);
        }

        /// <summary>
        /// Generate a short usage (arguments only) string
        /// </summary>
        public override string UsageShort()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (Command com in this.Commands.Values)
                sb.AppendLine(com.UsageShort());

            return sb.ToString();
        }

        /// <summary>
        /// Generate a medium usage (arguments and short description) string
        /// </summary>
        /// <param name="longestDescriptionLength">The length of the longest description in the group,
        /// the output is aligned to a nice table of one-line rows if possible.</param>
        public override string UsageMedium(int longestDescriptionLength)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (Command com in this.Commands.Values)
                sb.AppendLine(com.UsageMedium(longestDescriptionLength));

            return sb.ToString();
        }

        /// <summary>
        /// Run the command with the given arguments.
        /// The overload will be resolved and the real command will be ran.
        /// </summary>
        /// <param name="argText">The full command-line (without the command's name) containing the possible argument values</param>
        public new void Invoke(string argText)
        {
            if (this.Commands.Count == 0)
            {
                ConsoleExtensions.WriteColor("Cannot execute command '" + this.Verb + "', no known targets exist.", ConsoleColor.Red);
                return;
            }

            int argc = 0;
            try
            {
                argc = ParseArgumentLine(argText).Length;
            }
            catch (Exception e)
            {
                ConsoleExtensions.WriteColor("Unable to parse the given command.", ConsoleColor.Red);
                if (!String.IsNullOrWhiteSpace(e.Message))
                    Console.WriteLine(e.Message);
                return;
            }

            Command commandToRun = null;

            // Find the real command to execute based on the number of arguments given
            IEnumerable<Command> command_try = this.Commands.Values.Where(c => c.Arguments.Count == argc);
            if (command_try.Count() == 1)
                // Exact match, run that command
                commandToRun = command_try.First();
            else
            {
                Command maxArgCommand = this.Commands.Values.ElementAt(0);
                int maxArgCount = this.Commands.Values.ElementAt(0).Arguments.Count;
                {
                    if (this.Commands.Count > 1)
                        for (int i = 1; i < this.Commands.Count; ++i)
                            if (this.Commands.Values.ElementAt(i).Arguments.Count > maxArgCount)
                            {
                                maxArgCommand = this.Commands.Values.ElementAt(i);
                                maxArgCount = this.Commands.Values.ElementAt(i).Arguments.Count;
                            }
                }

                if (maxArgCount > 0)
                    // Check if there's a MultiParams argument in the command with the most arguments
                    if (maxArgCommand.Arguments[maxArgCommand.Arguments.Count - 1].MultiParams)
                        if (argc >= maxArgCommand.Arguments.Where(arg => arg.Mandatory).Count())
                            // If there is, call that one, the arguments above maxArgCount will be parsed as params
                            commandToRun = maxArgCommand;
            }

            if (commandToRun != null)
                commandToRun.Invoke(argText);
            else
            {
                ConsoleExtensions.WriteColor("Cannot match the given count of arguments to a particular command.", ConsoleColor.Red);
                Console.WriteLine("The following commands are available:");
                Console.WriteLine(this.UsageMedium(Command.CalculateLongestFittingDescriptionLength(this.Commands.Values)));
            }
        }
    }
}