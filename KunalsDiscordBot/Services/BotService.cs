using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Attributes;

namespace KunalsDiscordBot.Services
{
    public abstract class BotService
    {
        public static DiscordEmbedBuilder GetBotInfo(DiscordClient client, DiscordMember member, int thumbnailSize)
        {
            string modules = string.Empty;
            var allModules = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)));

            var embed = new DiscordEmbedBuilder
            {
                Title = "Hi! I'm Pepper",
                Description = $"**About Me:** I'm a girl and I love sleeping and eating.\n I'm in {client.Guilds.Count} server(s) and have {client.ShardCount} shard(s)." +
                $"The shard ID for this server is {client.ShardId}.",
                Color = DiscordColor.Blurple,
                Footer = GetEmbedFooter(member == null ? "Pepper" : $"User: {member.DisplayName} #{member.Discriminator}"),
                Thumbnail = GetEmbedThumbnail(client.CurrentUser, thumbnailSize)
            }.AddField("__The Modules I offer:__", "** **");

            foreach (var type in allModules)
                embed.AddField($"• {type.GetCustomAttribute<GroupAttribute>().Name}", "** **", true);

            embed.AddField($"** **", "** **")
                 .AddField("__My Prefix'__", "`pep`, `pepper`", true)
                 .AddField("__My Help Command__", "pep help", true)
                 .AddField("__Contribute__", "Use the `pep general github` command for the source code", true)
                 .AddField("__Configuration__", "Use the `pep general configuration` command to view and edit my configuration for this server", true)
                 .AddField("__Moderation and Solf Moderation__", "I also offer commands for server moderation, If you can't see them in the help command" +
                 ", its probably because I haven't been given the `Administrator` permission");

            return embed;
        }

        public static DiscordEmbedBuilder GetLeaveEmbed() => new DiscordEmbedBuilder
        {
            Title = "Had a great time here ppl!",
            Description = "Note: The config for this server will be deleted, in case I'm ever readded there would be a fresh new one",
            Footer = GetEmbedFooter($"Left server at {DateTime.Now}")
        };

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
