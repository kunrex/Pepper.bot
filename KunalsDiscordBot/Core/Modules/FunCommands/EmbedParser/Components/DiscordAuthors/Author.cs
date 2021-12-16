using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.CommonComponents;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordAuthors
{
    public class Author : EmbedComponent
    {
        public override bool Outer => true;
        public override string Id { get => "author"; }
        protected override Regex Regex { get; set; }

        private Name Name { get; set; }
        private AuthorIcon Icon { get; set; }
        private AuthorUrl Url { get; set; }

        public Author()
        {
            Regex = new Regex("(.*)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            var components = await new EmbedGenerator().ParseComponents(input);
            if (components.Count > 3 || components.Count < 1)
                return false;

            foreach (var component in components)
                if (!ExtractComponent(component))
                    return false;

            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder.WithAuthor(Name == null ? string.Empty : Name.Value, Url == null ? string.Empty : Url.Value, Icon == null ? string.Empty : Icon.Value);

        private bool ExtractComponent(EmbedComponent component)
        {
            if (!(component is Name) && !(component is AuthorIcon) && !(component is AuthorUrl))
                return false;

            switch (component.GetType())
            {
                case var x when x == typeof(Name):
                    if (Name != null)
                        return false;

                    Name = (Name)component;
                    break;
                case var x when x == typeof(AuthorIcon):
                    if (Icon != null)
                        return false;

                    Icon = (AuthorIcon)component;
                    break;
                case var x when x == typeof(AuthorUrl):
                    if (Url != null)
                        return false;

                    Url = (AuthorUrl)component;
                    break;
            }

            return true;
        }
    }
}
