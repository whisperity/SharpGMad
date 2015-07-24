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

        public void Invoke(string[] args)
        {
            if (this.Dispatch == null)
                return;

            if (this.IsAlias)
            {
                ((CommandAlias)this).Invoke(args);
                return;
            }

            RealtimeCommandline.WriteColor("Invoking command " + this.Verb +
                " with command-line arguments \"" + String.Join(";", args) + "\".", ConsoleColor.Yellow, true);
            // Bind all shell arguments to their values
            int i = 0;
            foreach (Argument arg in this.Arguments)
            {
                string stringArg = String.Empty;
                try
                {
                    stringArg = args[i++];
                }
                catch (IndexOutOfRangeException)
                {
                    if (arg.Mandatory)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("missing argument " + arg.Name);
                        Console.ResetColor();
                        Console.Write("Usage: " + this.Verb + " ");
                        foreach (Argument _arg in this.Arguments)
                            Console.Write((_arg.Mandatory ? "<" : "[") + _arg.Name + (_arg.Mandatory ? ">" : "]") + " ");
                        Console.WriteLine();

                        return; // Don't parse further
                    }
                }

                try
                {
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

        public new void Invoke(string[] args)
        {
            RealtimeCommandline.WriteColor("Invoking alias " + this.Verb + " for command " + this.RealVerb +
                " with command-line arguments \"" + String.Join(";", args) + "\".", ConsoleColor.Yellow, true);

            Command realCommand = this.ContextRegistry.Get(this.RealVerb);
            realCommand.Invoke(args);
        }
    }
}