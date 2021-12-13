using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components
{
    public class Description : EmbedComponent
    {
        public override bool Outer => true;
        public override string Id { get => "description"; }
        protected override Regex Regex { get; set; }

        public string Value { get; private set; }

        public Description()
        {
            Regex = new Regex("(.*)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            Value = input;
            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder.WithDescription(Value);
    }
}
