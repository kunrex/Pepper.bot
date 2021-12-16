using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordThumbnails
{
    public class Thumbnail : EmbedComponent
    {
        public override bool Outer => true;
        public override string Id { get => "thumbnail"; }
        protected override Regex Regex { get; set; }

        private ThumbnailIcon Icon { get; set; }
        private Dimensions Dimensions { get; set; }

        public Thumbnail()
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
            if (!(component is Dimensions) && !(component is ThumbnailIcon))
                return false;

            switch (component.GetType())
            {
                case var x when x == typeof(Dimensions):
                    if (Dimensions != null)
                        return false;

                    Dimensions = (Dimensions)component;
                    break;
                case var x when x == typeof(ThumbnailIcon):
                    if (Icon != null)
                        return false;

                    Icon = (ThumbnailIcon)component;
                    break;
            }

            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder.WithThumbnail(Icon == null ? string.Empty : Icon.Value, Dimensions == null ? 0 : Dimensions.Height, Dimensions == null ? 0 : Dimensions.Width);
    }
}
