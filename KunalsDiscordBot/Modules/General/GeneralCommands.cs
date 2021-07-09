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
using KunalsDiscordBot.Services;
using System.Reflection;

namespace KunalsDiscordBot.Modules.General
{
    [Group("General")]
    [Decor("Blurple", ":tools:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class GeneralCommands : BaseCommandModule
    {
        private static readonly DiscordColor Color = typeof(GeneralCommands).GetCustomAttribute<Decor>().color;
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

        [Command("poll")]
        [Description("Conducts a poll **DEMOCRACY**")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] reactions)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = reactions.Select(x => x.ToString());

            string poll = string.Empty;
            await ctx.Channel.SendMessageAsync($"What is the title of the poll?").ConfigureAwait(false);

            var message = await interactivity.WaitForMessageAsync(x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.Member.Id, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            if (message.TimedOut)
            {
                await ctx.Channel.SendMessageAsync($"Late response").ConfigureAwait(false);
                return;
            }
            else
                poll = message.Result.Content;

            var embed = new DiscordEmbedBuilder
            {
                Title = poll,
                Description = string.Join(" ", options),
                Color = Color
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            foreach (var option in reactions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);

            var results = result.Select(x => $"{x.Emoji} : {x.Total}");

            await ctx.Channel.SendMessageAsync($"\n Results for the pole: `{poll}` are -");
            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
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

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"User: {ctx.Member.Nickname}, at {DateTime.Now.ToString("dd hh:mm:ss.s")}"
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = member.Nickname == null ? $"{member.Username} (#{member.Discriminator})" : $"{member.Nickname} ({member.Username}) #{member.Discriminator}",
                Color = Color,
                Thumbnail = thumbnail,
                Footer = footer
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

        [Command("ServerInfo")]
        [Aliases("si")]
        [Description("Gives geenral information about the server")]
        public async Task ServerInfo(CommandContext ctx)
        {
            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = ctx.Guild.IconUrl,
                Height = Height,
                Width = Width
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"User: {ctx.Member.Nickname}, at {DateTime.Now.ToString("dd hh:mm:ss.s")}"
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = ctx.Guild.Name,
                Description = ctx.Guild.Description == string.Empty ? "None" : ctx.Guild.Description,
                Color = Color,
                Thumbnail = thumbnail,
                Footer = footer
            }.AddField("__General Info__", "** **")
             .AddField("Member Count", ctx.Guild.MemberCount.ToString(), true)
             .AddField("ID", ctx.Guild.Id.ToString(), true)
             .AddField("Region", ctx.Guild.VoiceRegion.Name.ToString(), true)
             .AddField("Owner", ctx.Guild.Owner.Username.ToString(), true)
             .AddField("__Roles, Emojis and Channels__", "** **")
             .AddField("Emoji Count", ctx.Guild.Emojis.Count.ToString(), true)
             .AddField("Roles Count", ctx.Guild.Roles.Count.ToString(), true)
             .AddField("Channel Count", ctx.Guild.Channels.Values.Where(x => !x.IsCategory).ToList().Count.ToString(), true)
             .AddField("__More__", "** **")
             .AddField("Verification Level", ctx.Guild.VerificationLevel.ToString(), true)
             .AddField("Nitro Tier", ctx.Guild.PremiumTier.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Ping")]
        [Description("Current ping of the client")]
        public async Task Ping(CommandContext ctx) => await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Ping",
            Description = $"Current latency is about {ctx.Client.Ping}ms",
            Color = Color
        }).ConfigureAwait(false);

        [Command("AboutMe")]
        [Description("Allow me to intorduce myself :D")]
        public async Task AboutMe(CommandContext ctx) => await ctx.Channel.SendMessageAsync(BotService.GetBotInfo(ctx.Client, ctx.Member, 30)).ConfigureAwait(false);
    }
}
