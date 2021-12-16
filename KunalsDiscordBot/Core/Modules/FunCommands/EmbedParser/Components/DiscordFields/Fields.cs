using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordFields
{
    public class Fields : EmbedComponent
    {
        public override string Id => "fields";
        public override bool Outer => true;
        protected override Regex Regex { get; set; }

        private List<Field> AllFields { get; set; }

        public Fields()
        {
            Regex = new Regex("(.*)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            var components = await new EmbedGenerator().ParseComponents(input);
            AllFields = new List<Field>();

            foreach (var field in components)
                if (!(field is Field casted))
                    return false;
                else
                    AllFields.Add(casted);
            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder)
        {
            if (AllFields == null)
                return builder;

            foreach (var value in AllFields)
                value.Modify(builder);

            return builder;
        }
    }
}
