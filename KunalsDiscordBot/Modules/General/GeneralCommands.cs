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
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Attributes.GeneralCommands;
using KunalsDiscordBot.Core.Exceptions;

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

        private readonly IServerService serverService;

        public GeneralCommands(IServerService service) => serverService = service;

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigPermsAttribute) != null;

            if (configPermsCheck)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.RestrictPermissionsToAdmin == 1 && (ctx.Member.PermissionsIn(ctx.Channel) & DSharpPlus.Permissions.Administrator) != DSharpPlus.Permissions.Administrator)
                {
                    await ctx.RespondAsync(":x: You need to be an admin to run this command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

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
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"The poll :{poll} has begun").ConfigureAwait(false);

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);

            var results = result.Select(x => $"{x.Emoji} : {x.Total}");

            await ctx.Channel.SendMessageAsync($"\n Results for the poll: `{poll}` are -");
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
                Text = $"User: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("dd hh:mm:ss.s")}"
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = member.Nickname == null ? $"{member.Username} (#{member.Discriminator})" : $"{member.Nickname} ({member.Username}) #{member.Discriminator}",
                Color = Color,
                Thumbnail = thumbnail,
                Footer = footer
            };

            embed.AddField("Account Created Date: ", $"{member.CreationTimestamp.Date.ToString("dddd, dd MMMM yyyy")}");
            embed.AddField("Server Join Date: ", $"{member.JoinedAt.Date.ToString("dddd, dd MMMM yyyy")}");
            embed.AddField("Id: ", $"`{member.Id}`");

            if (member.IsBot)
                embed.AddField("Is Bot:", "`true`");

            string roles = string.Empty;
            foreach (var role in member.Roles)
                roles += $"<@&{role.Id}>\n";

            embed.AddField("Roles: ", roles == string.Empty ? "None" : roles);

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

        [Command("Config")]
        public async Task Config(CommandContext ctx)
        {
            var profile = await serverService.GetServerProfile(ctx.Guild.Id);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Configuration",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}"),
                Color = Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.Client.CurrentUser, 30)
            }.AddField("__General__", "** **")
             .AddField($"Only Allow Admins To Edit Config: `{profile.RestrictPermissionsToAdmin == 1}`", "When set to true only admins can edit the configuration")
             .AddField($"Log Errors: `{profile.LogErrors == 1}`", "When set to true, a message is sent if an error happens during command execution")
             .AddField($"Log New Members: `{profile.LogNewMembers == 1}`", "When set to true, a message is sent if a new member joins or or a member leaves the server")
             .AddField($"Log Channel:", $" {(profile.LogChannel == 0 ? "`None`" : "<@#{(ulong)profile.RulesChannelId}>")}\nThe log channel of the server, or the channel in which welcome messages are sent");

            var permissions = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).PermissionsIn(ctx.Channel);

            var enabled = (permissions & DSharpPlus.Permissions.Administrator) == DSharpPlus.Permissions.Administrator;
            embed.AddField("__Moderation and Soft Moderation__", "** **").AddField($"Enabled: `{enabled}`",
                "Wether or not the moderation and soft moderation modules are enabled in thi server");

            if (enabled)
            {
                var ruleCount = (await serverService.GetAllRules(ctx.Guild.Id)).Count;

                embed.AddField($"Muted Role:", $"{(profile.MutedRoleId == 0 ? "`None`" : $"<@&{(ulong)profile.MutedRoleId}>")}\nThe role that ias assigned when a member is muted", true);
                embed.AddField($"Rule Count: `{ruleCount}`", "The amount of rules in the server", true);
                embed.AddField($"Rule Channel:", $"{(profile.RulesChannelId == 0 ? "`None`" : $"<@#{(ulong)profile.RulesChannelId}>")}\nThe amount of rules in the server", true);
                embed.AddField($"Moderator Role:", $"{(profile.ModeratorRoleId == 0 ? "`None`" : $" <@&{(ulong)profile.ModeratorRoleId}>")}\nThe moderator role of this server", true);
            }

            embed.AddField("__Music__", "** **")
                 .AddField($"Enforce DJ Permissions: `{(profile.UseDJRoleEnforcement == 1)}`", "When set to true, a member cannot run most music commands without the DJ role");

            if (profile.UseDJRoleEnforcement == 1)
                embed.AddField($"DJ Role:", $"{(profile.DJRoleId == 0 ? "`None`" : $" <@&{(ulong)profile.DJRoleId}>")}The DJ role for this server", true);

            embed.AddField("__Fun__", "** **").AddField($"Allow NSFW: `{(profile.AllowNSFW == 1)}`", "When set to true the bot can post NSFW posts from NSFW subreddits".ToString());

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("ChangeEditPermissions")]
        [Aliases("ConfigPerms", "Perms")]
        [Description("Changes if only an administrator can change the config data of the bot per server. This command can only be run by an Administrator")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task ToggleAdminPermission(CommandContext ctx, bool toChange)
        {
            await serverService.ToggleNSFW(ctx.Guild.Id, toChange).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce Admin Permissions For Editing Config` to {toChange}",
                Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("ToggleNSFW")]
        [Aliases("NSFW")]
        [Description("Changes wether or not NSFW content is allowed in the server")]
        [CheckConfigPerms]
        public async Task ToggleNSFW(CommandContext ctx, bool toChange)
        {
            await serverService.ToggleNSFW(ctx.Guild.Id, toChange).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow NSFW` to {toChange}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("ToggleDJ")]
        [Aliases("DJ")]
        [Description("Changes wether or not the DJ role should be enforced for music commands")]
        [CheckConfigPerms]
        public async Task ToggeDJ(CommandContext ctx, bool toChange)
        {
            await serverService.ToggleDJOnly(ctx.Guild.Id, toChange).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce DJ Role` to {toChange}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("DJRole")]
        [Description("Assigns the DJ role for a server")]
        [CheckConfigPerms]
        public async Task DJRole(CommandContext ctx, DiscordRole role)
        {
            var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);
            if(profile.UseDJRoleEnforcement == 0)
            {
                await ctx.RespondAsync("`Enforce DJ Role` must be set to true to use this command, you can do so using the `general ToggleDJ` command").ConfigureAwait(false);
                return;
            }

            await serverService.SetDJRole(ctx.Guild.Id, role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce DJ Role` to {role.Mention}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("LogErrors")]
        [Description("If true, a message is sent if an error happens during command execution")]
        [CheckConfigPerms]
        public async Task LogErrors(CommandContext ctx, bool toSet)
        {
            await serverService.ToggleLogErrors(ctx.Guild.Id, toSet).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Log Errors` to {toSet}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("LogNewMembers")]
        [Description("If true, a message is sent if a new member joins or or a member leaves the server")]
        [CheckConfigPerms]
        public async Task LogNewMembers(CommandContext ctx, bool toSet)
        {
            await serverService.ToggleNewMemberLog(ctx.Guild.Id, toSet).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Log New Members` to {toSet}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("LogChannel")]
        [Description("Assigns the log channel for a server")]
        [CheckConfigPerms]
        public async Task LogChannel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetLogChannel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the log channel for guild: {ctx.Guild.Name}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("RuleChannel")]
        [Description("Assigns the rule channel for a server")]
        [CheckConfigPerms]
        public async Task RuleChannel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetRuleChannel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the rule channel for guild: {ctx.Guild.Name}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }
    }
}
