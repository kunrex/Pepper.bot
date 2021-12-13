using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components
{
    public class Color : EmbedComponent
    {
        public override bool Outer => true;
        public override string Id { get => "color"; }
        protected override Regex Regex { get; set; }

        private DiscordColor Value { get; set; }

        public Color()
        {
            Regex = new Regex("(#[0-9]{6})|([A-z]+)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            if (input[0] == '#')
                Value = new DiscordColor(input);
            else
            {
                var property = typeof(DiscordColor).GetProperty(char.ToUpper(input[0]) + input.Substring(1));
                if (property == null)
                    return false;

                Value = (DiscordColor)property.GetValue(null, null); 
            }

            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder.WithColor(Value);
    }
}
