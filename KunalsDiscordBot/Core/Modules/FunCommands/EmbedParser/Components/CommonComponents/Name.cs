using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.CommonComponents
{
    public class Name : EmbedComponent
    {
        public override bool Outer => false;
        public override string Id { get => "name"; }
        protected override Regex Regex { get; set; }

        public string Value { get; private set; }

        public Name()
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
    }
}
