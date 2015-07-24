using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGMad.Shell
{
    class Command
    {
        public string Verb { get; protected set; }
        public string Description;
        public List<Argument> Arguments;
        protected Action<Command> Dispatch;

        protected bool IsAlias;

        protected Command()
        {
            this.Arguments = new List<Argument>();
            this.IsAlias = false;
        }

        public Command(string verb, Action<Command> dispatch)
            : this()
        {
            this.Verb = verb;
            this.Dispatch = dispatch;
        }

        public Argument GetArgument(string name)
        {
            return this.Arguments.Where(arg => arg.Name == name).FirstOrDefault();
        }

        private bool GetArgumentPart(string args, out string result)
        {
            // To store the current argument
            result = String.Empty;

            if (args.Length == 0)
                return false;

            if (args[0] == '"')
            {
                int nextQuote = 1;
                bool nextQuoteFound = false;

                while (!nextQuoteFound)
                {
                    nextQuote = args.IndexOf('"', nextQuote + 1); // Search for the next quote mark
                    if (nextQuote != -1)
                    {
                        if (args[nextQuote - 1] != '\\') // If the next quote symbol is escaped, skip it
                            nextQuoteFound = true;
                    }
                    else
                    {
                        ConsoleExtensions.WriteColor("malformed argument, the quoted block is not closed properly", ConsoleColor.Red);
                        throw new ArgumentException("Malformed argument, the quoted block is not closed properly");
                    }
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

                return GetArgumentPart(args.Substring(c), out result);
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
                {
                    ConsoleExtensions.WriteColor("malformed argument, found unescaped quote sequence", ConsoleColor.Red);
                    Console.WriteLine("if you wish to use \" symbols in your text, escape them by writing \\\"");
                    throw new ArgumentException("Malformed argument, found an unescaped quote sequence.");
                }
            }

            return true;
        }

        public string ShortUsageString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(this.Verb + " ");
            foreach (Argument arg in this.Arguments)
                sb.Append(
                    (arg.Mandatory ? "<" : "[")
                    + arg.Name
                    + (arg.MultiParams ? "..." : String.Empty)
                    + (arg.Mandatory ? ">" : "]")
                    + " "
                );

            return sb.ToString();
        }

        public void Invoke(string argText)
        {
            if (this.Dispatch == null)
                return;

            if (this.IsAlias)
            {
                ((CommandAlias)this).Invoke(argText);
                return;
            }

            // Bind all shell arguments to their values
            // This is done by consuming the argText variable containing all the shell arguments
            int i = 0;
            foreach (Argument arg in this.Arguments)
            {
                // If the argument is a "params" one, run it as much as we can.
                bool keepRunning = true;

                while (keepRunning)
                {
                    string stringArg = String.Empty;

                    try
                    {
                        if (!GetArgumentPart(argText, out stringArg))
                            throw new IndexOutOfRangeException(); // If we are unable to parse the argument, signal it and stop trying
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
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // We need to show an error message if the argument is mandatory and if it does not have a value
                        if (arg.Mandatory && !arg.HasValue)
                        {
                            ConsoleExtensions.WriteColor("missing argument " + arg.Name, ConsoleColor.Red);
                            Console.WriteLine("Usage: " + this.ShortUsageString());

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
                    catch (ArgumentException)
                    {
                        // If there was a problem with the argument, don't invoke the command
                        return;
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
            foreach (Argument arg in this.Arguments)
                arg.Reset();
        }
    }

    class CommandAlias : Command
    {
        private CommandRegistry ContextRegistry;
        private string RealVerb;

        public CommandAlias(string aliasedVerb, string realVerb, CommandRegistry context)
        {
            Command realCommand = context.Get(realVerb);

            base.Verb = aliasedVerb;
            this.RealVerb = realVerb;
            base.Description = "Alias for command " + realVerb;
            base.IsAlias = true;
            base.Arguments = new List<Argument>(realCommand.Arguments);

            this.ContextRegistry = context;
            this.Dispatch = (self) =>
            {
                throw new InvalidOperationException("Error: The dispatch for an alias method should NEVER be called...");
            };
        }

        public new void Invoke(string argText)
        {
            Command realCommand = this.ContextRegistry.Get(this.RealVerb);
            realCommand.Invoke(argText);
        }
    }
}