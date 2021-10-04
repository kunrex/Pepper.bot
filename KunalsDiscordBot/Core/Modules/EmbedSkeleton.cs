using System;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Extensions
{
    public struct EmbedSkeleton
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public DiscordColor Color { get; set; }

        public DiscordEmbedBuilder.EmbedFooter Footer { get; set; }
        public DiscordEmbedBuilder.EmbedThumbnail Thumbnail { get; set; }
        public DiscordEmbedBuilder.EmbedAuthor Author { get; set; }
    }
}
