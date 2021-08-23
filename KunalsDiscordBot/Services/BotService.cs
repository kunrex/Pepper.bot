using System;
using System.Linq;
using System.Reflection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace KunalsDiscordBot.Services
{
    public abstract class BotService
    {
        public static DiscordEmbedBuilder GetLeaveEmbed() => new DiscordEmbedBuilder
        {
            Title = "Had a great time here ppl!",
            Description = "Note: The config for this server will be deleted, in case I'm ever readded there would be a fresh new one",
        }.WithFooter($"Left server at {DateTime.Now}");

        public static DiscordEmbedBuilder.EmbedThumbnail GetEmbedThumbnail(DiscordUser user, int thumbnailSize) => new DiscordEmbedBuilder.EmbedThumbnail
        {
            Url = user.AvatarUrl,
            Height = thumbnailSize,
            Width = thumbnailSize
        };

        public static DiscordEmbedBuilder.EmbedFooter GetEmbedFooter(string text) => new DiscordEmbedBuilder.EmbedFooter
        {
            Text = text
        };
    }
}
