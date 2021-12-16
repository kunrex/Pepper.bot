using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.CommonComponents;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordFooters
{
    public class Footer : EmbedComponent
    {
        public override bool Outer => true;
        public override string Id { get => "footer"; }
        protected override Regex Regex { get; set; }

        private Value Value { get; set; }
        private FooterIcon Icon { get; set; }

        public Footer()
        {
            Regex = new Regex("(.*)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            var components = await new EmbedGenerator().ParseComponents(input);
            if (components.Count > 2 || components.Count < 1)
                return false;

            foreach (var component in components)
                if (!ExtractComponent(component))
                    return false;

            return true;
        }

        private bool ExtractComponent(EmbedComponent component)
        {
            if (!(component is Value) && !(component is FooterIcon))
                return false;

            switch (component.GetType())
            {
                case var x when x == typeof(Value):
                    if (Value != null)
                        return false;

                    Value = (Value)component;
                    break;
                case var x when x == typeof(FooterIcon):
                    if (Icon != null)
                        return false;

                    Icon = (FooterIcon)component;
                    break;
            }

            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder.WithFooter(Value == null ? string.Empty : Value.StringValue, Icon == null ? string.Empty : Icon.Value);
    }
}
