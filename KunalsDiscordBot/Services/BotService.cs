using System;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Services
{
    public abstract class BotService
    {
        public static DiscordEmbedBuilder GetLeaveEmbed() => new DiscordEmbedBuilder
        {
            Title = "Had a great time here ppl!",
            Description = "Note: The config for this server will be deleted, in case I'm ever readded there would be a fresh new one",
        }.WithFooter($"Left server at {DateTime.Now}");
    }
}
