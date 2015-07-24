using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGMad.Shell
{
    /// <summary>
    /// Holds a set of commands.
    /// </summary>
    class CommandRegistry
    {
        private List<Command> Commands;

        public CommandRegistry()
        {
            this.Commands = new List<Command>();
        }

        public void Add(Command com)
        {
            IEnumerable<Command> search = this.Commands.Where(c => c.Verb == com.Verb);
            if (search.Count() != 0)
                throw new ArgumentException("A command with the verb '" + com.Verb + "' is already added.");

            this.Commands.Add(com);
        }

        /// <summary>
        /// Creates an alias for the command realVerb as aliasVerb. Running aliasVerb will execute realVerb.
        /// </summary>
        /// <param name="realVerb">The command to provide alias for</param>
        /// <param name="aliasVerb">The alias to register</param>
        public void AddAlias(string realVerb, string aliasVerb)
        {
            CommandAlias alias = new CommandAlias(aliasVerb, realVerb, this);
            this.Commands.Add(alias);
        }

        public void Delete(string verb)
        {
            Command com = this.Get(verb);
            this.Commands.Remove(com);
        }

        public bool Exists(string verb)
        {
            IEnumerable<Command> search = this.Commands.Where(c => c.Verb == verb);
            return (search.Count() == 1);
        }

        public Command Get(string verb)
        {
            IEnumerable<Command> search = this.Commands.Where(c => c.Verb == verb);
            if (search.Count() == 0)
                throw new ArgumentException("A command with the verb '" + verb + "' does not exist.");

            return search.First();
        }
    }
}
