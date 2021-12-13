using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordFields
{
    public class Inline : EmbedComponent
    {
        public override bool Outer => false;
        public override string Id { get => "inline"; }
        protected override Regex Regex { get; set; }

        public bool Value { get; private set; }

        public Inline()
        {
            Regex = new Regex("^(true|false)$");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            Value = bool.Parse(input);
            return true;
        }
    }
}
