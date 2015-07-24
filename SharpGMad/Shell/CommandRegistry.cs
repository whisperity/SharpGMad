using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGMad.Shell
{
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

        public void AddAlias(string aliasVerb, string realVerb)
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
