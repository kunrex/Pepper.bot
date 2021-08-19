using System;
using System.Linq;
using System.Threading.Tasks;


using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Configuration;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Modules.General
{
    [Group("General")]
    [Decor("Blurple", ":tools:")]
    [Description("General commands. This inlcudes viewing server configuration.")]
    [ModuleLifespan(ModuleLifespan.Transient), ConfigData(ConfigValueSet.General)]
    public class GeneralCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private const int Height = 15;
        private const int Width = 20;

        private readonly IServerService serverService;
        private readonly IConfigurationService configService;

        public GeneralCommands(IServerService service, IConfigurationService _configService, ModuleService moduleService)
        {
            serverService = service;
            configService = _configService;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.General];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigigurationPermissionsAttribute) != null;

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

        [Command("Githhub")]
        [Description("Source Code for Pepper.bot")]
        public async Task GitHub(CommandContext ctx) => await ctx.Channel.SendMessageAsync("Heres my source code! https://github.com/kunrex/Pepper.bot");

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
                Color = ModuleInfo.Color
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
                Color = ModuleInfo.Color,
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
                Color = ModuleInfo.Color,
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
        public async Task Ping(CommandContext ctx)
        {
            var time = DateTime.Now;
            var messsage = await ctx.Channel.SendMessageAsync("Collecting").ConfigureAwait(false);

            var difference = DateTime.Now - time;
            await messsage.DeleteAsync();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Pong! :ping_pong:",
                Color = ModuleInfo.Color
            }.AddField("API Latency", $"{ctx.Client.Ping}ms")
             .AddField("Client Latency", $"{difference.Milliseconds}ms")).ConfigureAwait(false);
        }

        [Command("AboutMe")]
        [Description("Allow me to intorduce myself :D")]
        public async Task AboutMe(CommandContext ctx) => await ctx.Channel.SendMessageAsync(BotService.GetBotInfo(ctx.Client, ctx.Member, 30)).ConfigureAwait(false);

        [Command("Config")]
        public async Task Config(CommandContext ctx)
        {
            var footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}. Message will collect reactions for 2 minutes");
            var thumbnail = BotService.GetEmbedThumbnail(ctx.Client.CurrentUser, 30);

            var embeds = await configService.GetConfigPages(ctx.Guild.Id, ctx.Member.PermissionsIn(ctx.Channel));

            var pages = new Page[embeds.Count];
            for (int i = 0; i < embeds.Count; i++)
            {
                embeds[i].Footer = footer;
                embeds[i].Thumbnail = thumbnail;
                embeds[i].Color = ModuleInfo.Color;

                pages[i] = new Page($"Configuration for `{ctx.Guild.Name}`", embeds[i]);
            }

            var emojis = new PaginationEmojis
            {
                Left = DiscordEmoji.FromName(ctx.Client, ":arrow_backward:"),
                Right = DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"),
                SkipLeft = null,
                SkipRight = null,
                Stop = null
            };

            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, emojis, DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround, DSharpPlus.Interactivity.Enums.PaginationDeletion.DeleteEmojis, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
        }

        [Command("ChangeEditPermissions")]
        [Aliases("ConfigPerms", "Perms")]
        [Description("Changes if only an administrator can change the config data of the bot per server. This command can only be run by an Administrator")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        [ConfigData(ConfigValue.EnforcePermissions)]
        public async Task ToggleAdminPermission(CommandContext ctx, bool toChange)
        {
            await serverService.TogglePermissions(ctx.Guild.Id, toChange).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce Admin Permissions For Editing Config` to {toChange}",
                Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }

        [Command("LogErrors")]
        [Description("If true, a message is sent if an error happens during command execution")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.LogErrors)]
        public async Task LogErrors(CommandContext ctx, bool toSet)
        {
            await serverService.ToggleLogErrors(ctx.Guild.Id, toSet).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Log Errors` to {toSet}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }

        [Command("LogNewMembers")]
        [Description("If true, a message is sent if a new member joins or or a member leaves the server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.LogMembers)]
        public async Task LogNewMembers(CommandContext ctx, bool toSet)
        {
            await serverService.ToggleNewMemberLog(ctx.Guild.Id, toSet).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Log New Members` to {toSet}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }

        [Command("WelcomeChannel")]
        [Description("Assigns the log channel for a server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.WelcomeChannel)]
        public async Task WelcomeChannel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetWelcomeChannel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the welcome channel for guild: `{ctx.Guild.Name}`",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }

        [Command("RuleChannel")]
        [Description("Assigns the log channel for a server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.RuleChannel)]
        public async Task RuleChannel(CommandContext ctx, DiscordChannel channel)
        {
            await serverService.SetRuleChannel(ctx.Guild.Id, channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the rule channel for guild: `{ctx.Guild.Name}`",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }
    }
}
