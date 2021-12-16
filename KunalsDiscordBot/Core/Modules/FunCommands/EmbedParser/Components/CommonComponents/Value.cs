using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.CommonComponents
{
    public class Value : EmbedComponent
    {
        public override bool Outer => false;
        public override string Id { get => "value"; }
        protected override Regex Regex { get; set; }

        public string StringValue { get; private set; }

        public Value()
        {
            Regex = new Regex("(.*)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            StringValue = input;
            return true;
        }
    }
}
