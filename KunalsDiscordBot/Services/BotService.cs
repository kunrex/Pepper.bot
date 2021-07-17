using System;
using System.Linq;
using System.Reflection;
using DiscordBotDataBase.Dal.Models.Servers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KunalsDiscordBot.Services.General;

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
                Description = "**About Me:** I'm a girl and I love sleeping and eating.\n",
                Color = DiscordColor.Blurple,
                Footer = GetEmbedFooter(member == null ? "Pepper" : $"User: {member.DisplayName} #{member.Discriminator}"),
                Thumbnail = GetEmbedThumbnail(client.CurrentUser, thumbnailSize)
            }.AddField("__The Modules I offer:__", "** **");

            foreach (var type in allModules)
                embed.AddField($"• {type.GetCustomAttribute<GroupAttribute>().Name}", "** **", true);

            embed.AddField($"** **", "** **")
                 .AddField("__My Prefix'__", "`pep`, `pepper`", true)
                 .AddField("__My Help Command__", "pep help", true)
                 .AddField("__Configuration__", "use the `pep general configuration` command to view and edit my configuration for this server", true)
                 .AddField("__Contribute__", "The githib repo isn't public yet", true)
                 .AddField("__Moderation and Solf Moderation__", "I also offer commands for server moderation, If you can't see them in the help command" +
                 ", its probably because I haven't been given the `Administrator` permission");

            return embed;
        }

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
