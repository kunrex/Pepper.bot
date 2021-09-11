using System;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public struct MessageData
    {
        public DiscordColor Color { get; set; }

        public DiscordEmbedBuilder.EmbedThumbnail Thumbnail { get; set; }
        public DiscordEmbedBuilder.EmbedFooter Footer { get; set; }
        public DiscordEmbedBuilder.EmbedAuthor Author { get; set; }

        public bool Reply { get; set; }
        public ulong ReplyId { get; set; }
    }
}
