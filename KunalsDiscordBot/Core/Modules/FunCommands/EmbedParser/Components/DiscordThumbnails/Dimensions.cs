using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordThumbnails
{
    public class Dimensions : EmbedComponent
    {
        public override string Id => "dimensions";
        public override bool Outer => false;
        protected override Regex Regex { get; set; }

        public int Height { get; private set; }
        public int Width { get; private set; }

        public Dimensions()
        {
            Regex = new Regex("^(([0-9][0-9]?),([0-9][0-9]?))$");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            var numbers = input.Split(',').Select(x => int.Parse(x)).ToArray();
            Height = numbers[0];
            Width = numbers[1];

            return true;
        }
    }
}
