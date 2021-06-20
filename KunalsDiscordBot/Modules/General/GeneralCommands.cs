//System name spaces
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Attributes;

namespace KunalsDiscordBot.Modules.General
{
    [Group("General")]
    [Decor("Blurple", ":tools:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class GeneralCommands : BaseCommandModule
    {
        private const int Height = 50;
        private const int Width = 75;

        [Command("date")]
        [Description("Tells you the date")]
        public async Task Date(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(DateTime.Now.ToString("dddd, dd MMMM yyyy")).ConfigureAwait(false);
        }


        [Command("time")]
        [Description("Tells you the time")]
        public async Task Time(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(DateTime.Now.ToString("hh:mm tt")).ConfigureAwait(false);
        }

        [Command("describe")]
        [Description("Random charecter dexcription in one word")]
        public async Task charecter(CommandContext ctx, string name)
        {
            if (name.ToLower().Equals("rysa"))
                await ctx.Channel.SendMessageAsync("thanush");

            string[] replies = { "sweat", "memes", "god", "assasin", "simp", "trash", "legend" };
            Random r = new Random();
            int rand = r.Next(0, replies.Length - 1);

            await ctx.Channel.SendMessageAsync(replies[rand]).ConfigureAwait(false);
        }


        [Command("poll")]
        [Description("Conducts a poll **DEMOCRACY**")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, string poll, params DiscordEmoji[] reactions)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = reactions.Select(x => x.ToString());

            Console.WriteLine(options);
            var Embed = new DiscordEmbedBuilder
            {
                Title = poll,
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: Embed).ConfigureAwait(false);

            foreach (var option in reactions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);

            var results = result.Select(x => $"{x.Emoji} : {x.Total}");

            await ctx.Channel.SendMessageAsync("\n Results for the pole " + poll + " are ");
            await ctx.Channel.SendMessageAsync(string.Join("\n-", results)).ConfigureAwait(false);
        }

        [Command("UserInfo")]
        [Aliases("ui")]
        [Description("Gives general information about a user")]
        public async Task UserInfo(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = member.AvatarUrl,
                Height = Height,
                Width = Width
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = member.Nickname == null ? $"{member.Username} (#{member.Discriminator})" : $"{member.Nickname} ({member.Username}) #{member.Discriminator}",
                Color = DiscordColor.Blurple,
                Thumbnail = thumbnail
            };

            embed.AddField("Account Created Date: ", $"{member.CreationTimestamp.Date}");
            embed.AddField("Server Join Date: ", $"{member.JoinedAt.Date}");
            embed.AddField("Id: ", $"`{member.Id}`");

            if (member.IsBot)
                embed.AddField("Is Bot:", "`true`");

            string roles = string.Empty;
            foreach (var role in member.Roles)
                roles += $"<@&{role.Id}>\n";

            embed.AddField("Roles: ", roles);

            await ctx.Channel.SendMessageAsync(embed: embed);
        }
    }
}
