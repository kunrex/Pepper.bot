using System;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordThumbnails
{
    public class ThumbnailIcon : ImageUrl
    {
        public override bool Outer => false;
        public override string Id { get => "thumbnailicon"; }

        public ThumbnailIcon()
        {
            Regex = new Regex("(.*)");
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder;
    }
}
