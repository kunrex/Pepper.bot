using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components
{
    public abstract class EmbedComponent
    {        
        public abstract string Id { get; }
        protected abstract Regex Regex { get; set; }
        public abstract bool Outer { get; }

        protected string input;

        public EmbedComponent()
        {
            
        }

        public EmbedComponent WithInput(string _input)
        {
            input = _input;
            return this;
        }

        public virtual Task<bool> MatchAndExtract() => Task.FromResult(Regex.IsMatch(input));
        public virtual DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder;
    }
}
