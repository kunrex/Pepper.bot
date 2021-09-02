using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Modules.Moderation
{
    [Group("Moderation")]
    [Aliases("Mod")]
    [Decor("Blurple", ":scales:")]
    [Description("Moderation Commands")]
    [RequireBotPermissions(Permissions.Administrator), ConfigData(ConfigValueSet.Moderation)]
    public class ModerationCommands : PepperCommandModule
    {
        private static readonly int ThumbnailSize = 20;

        public override PepperCommandModuleInfo ModuleInfo { get; protected set; } 

        private readonly IModerationService modService;
        private readonly IServerService serverService;

        public ModerationCommands(IModerationService moderationService, IServerService _serverService, IModuleService moduleService)
        {
            modService = moderationService;
            serverService = _serverService;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Moderation];
        }

        [Command("AddRole")]
        [Aliases("ar")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddRole(CommandContext ctx, DiscordRole role, DiscordMember member)
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (botMember.GetHighest() < role.Position)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"The role {role.Mention}, is higher than my higher role. Thus I cannot add or remove it.",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("Member already has the specified role");
                return;
            }

            await member.GrantRoleAsync(role).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Added Role",
                Color = ModuleInfo.Color,
            }.AddField("Role: ", role.Mention)
             .AddField("To: ", member.Mention)
             .WithFooter($"Admin: { ctx.Member.DisplayName} #{ctx.Member.Discriminator}")
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
        }

        [Command("RemoveRole")]
        [Aliases("rr")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveRole(CommandContext ctx, DiscordRole role, DiscordMember member)
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if (botMember.GetHighest() < role.Position)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"The role {role.Mention}, is higher than my higher role. Thus I cannot add or remove it.",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            if (member.Roles.First(x => x.Id == role.Id) == null)
            {
                await ctx.RespondAsync("Member does not have the specified role");
                return;
            }

            await member.RevokeRoleAsync(role).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Removed Role",
                Color = ModuleInfo.Color,
            }.AddField("Role: ", role.Mention)
             .AddField("From: ", member.Mention)
             .WithFooter($"Admin: { ctx.Member.DisplayName}, at {DateTime.Now}")
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
        }

        [Command("TemporaryBan")]
        [Description("Bans a member and unbans the member after a preiod of time")]
        [RequireBotPermissions(Permissions.BanMembers), Aliases("Tempban")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task BanMember(CommandContext ctx, DiscordMember member, TimeSpan timePreiod, [RemainingText] string reason = "Unspecified")
        {
            try
            {
                if (await ctx.Guild.GetMemberAsync(member.Id) == null)
                {
                    await ctx.RespondAsync("User isn't the server?");
                    return;
                }
                if ((await ctx.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == member.Id) == null)
                {
                    await ctx.RespondAsync("User is banned?");
                    return;
                }

                await member.BanAsync(5, reason).ConfigureAwait(false);
                int id = await modService.AddBan(member.Id, ctx.Guild.Id, ctx.Member.Id, reason, timePreiod.ToString());

                BotEventFactory.CreateScheduledEvent().WithSpan(timePreiod).WithEvent(async(s, e) =>
                {
                    if((await ctx.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == member.Id) != null)
                        await ctx.Guild.UnbanMemberAsync(member);
                }).Execute();

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = $"Banned member {member.Username} [Id: {id}]",
                    Color = ModuleInfo.Color,
                }.AddField("Reason: ", reason)
                 .WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")
                 .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Failed to ban specified member").ConfigureAwait(false);
            }
        }

        [Command("Ban")]
        [Description("Bans a member")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task BanMember(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unspecified")
        {
            try
            {
                if (await ctx.Guild.GetMemberAsync(member.Id) == null)
                {
                    await ctx.RespondAsync("User isn't the server?");
                    return;
                }
                if ((await ctx.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == member.Id) != null)
                {
                    await ctx.RespondAsync("User is banned?");
                    return;
                }

                await member.BanAsync(5, reason).ConfigureAwait(false);
                int id = await modService.AddBan(member.Id, ctx.Guild.Id, ctx.Member.Id, reason, TimeSpan.FromDays(0).ToString());

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = $"Banned member {member.Username} [Id: {id}]",
                    Color = ModuleInfo.Color,
                }.AddField("Reason: ", reason)
                 .WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")
                 .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
            }
            catch 
            {
                await ctx.Channel.SendMessageAsync($"Failed to unban specified user").ConfigureAwait(false);
            }
        }

        [Command("Unban")]
        [Description("Unbans a member")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task UnBanMember(CommandContext ctx, DiscordUser user,[RemainingText] string reason = "Unspecified")
        {
            if ((await ctx.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == user.Id) == null)
            {
                await ctx.RespondAsync("User isn't banned?");
                return;
            }

            await ctx.Guild.UnbanMemberAsync(user, reason).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Unbanned user {user.Username} #{user.Discriminator}",
                Color = ModuleInfo.Color,
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")
             .WithThumbnail(user.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
        }

        [Command("Kick")]
        [Description("Kicks a member")]
        [RequireBotPermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task KickMember(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unspecified")
        {
            try
            {
                await member.RemoveAsync(reason).ConfigureAwait(false);
                int id = await modService.AddKick(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = $"Kicked member {member.Username} [Id: {id}]",
                    Color = ModuleInfo.Color,
                }.AddField("Reason: ", reason)
                 .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)
                 .WithFooter($"Admin: {ctx.Member.Discriminator} at {DateTime.Now}")).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Failed to kick specified member").ConfigureAwait(false);
            }
        }

        [Command("GetKick")]
        [Description("Gets a kick event using its ID")]
        public async Task GetKick(CommandContext ctx, int kickID)
        {
            var kick = await modService.GetKick(kickID);

            if (kick == null)
            {
                await ctx.Channel.SendMessageAsync("Kick with this Id doesn't exist");
                return;
            }

            if ((ulong)kick.GuildID != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Kick with this Id doesn't exist exist in this server");
                return;
            }

            var admin = await ctx.Guild.GetMemberAsync((ulong)kick.ModeratorID).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Kick {kick.Id}",
                Description = $"User: <@{(ulong)kick.UserId}>",
                Color = ModuleInfo.Color,
            }.AddField("Reason", kick.Reason)
             .WithFooter($"Admin: {(admin == null ? admin.DisplayName : "admin isn't in the server anymore")}")).ConfigureAwait(false);
        }

        [Command("GetBan")]
        [Description("Gets a kick event using its ID")]
        public async Task GetBan(CommandContext ctx, int banID)
        {
            var ban = await modService.GetBan(banID);

            if (ban == null)
            {
                await ctx.Channel.SendMessageAsync("Ban with this Id doesn't exist");
                return;
            }

            if ((ulong)ban.GuildID != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Ban with this Id doesn't exist exist in this server");
                return;
            }

            var admin = await ctx.Guild.GetMemberAsync((ulong)ban.ModeratorID).ConfigureAwait(false);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Ban {ban.Id}",
                Description = $"User: <@{(ulong)ban.UserId}>",
                Color = ModuleInfo.Color,
            }.AddField("Reason", ban.Reason)
             .WithFooter($"Admin: {(admin == null ? admin.DisplayName : "admin isn't in the server anymore")}");

            var span = TimeSpan.Parse(ban.Time);
            embed.AddField("Delete Message Time: ", span == TimeSpan.FromDays(0) ? "Forever" : $"{span.Days} days, {span.Hours} hours, {span.Minutes} minutes, {span.Seconds} seconds");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("RemoveAllRoles")]
        [Aliases("rar")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveAllRoles(CommandContext ctx, DiscordMember member, string reason = "Unspecified")
        {
            int botHighest = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).Hierarchy;
            List<string> rolesRemoved = new List<string>();

            foreach (var role in member.Roles)
            {
                if (role.Position >= botHighest)
                    continue;

                await member.RevokeRoleAsync(role).ConfigureAwait(false);
                rolesRemoved.Add(role.Mention);
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Removed all roles (that I can)",
                Color = ModuleInfo.Color,
            }.AddField("Roles", rolesRemoved.Count == 0 ? "None" : string.Join(',', rolesRemoved.Select(x => $"`{x}`")))
             .AddField("From: ", member.Mention)
             .AddField("Reason: ", reason)).ConfigureAwait(false);
        }

        [Command("MemberInfo")]
        [Description("Gets moderation info about the member")]
        public async Task MemberInfo(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"{member.DisplayName} # {member.Discriminator}",
                Color = ModuleInfo.Color,
            }.AddField("Infractions: ", (await modService.GetInfractions(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .AddField("Endorsements: ", (await modService.GetEndorsements(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .AddField("** **", "** **")
             .AddField("Bans: ", (await modService.GetBans(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .AddField("Kicks: ", (await modService.GetKicks(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .WithFooter($"User: {ctx.User.Discriminator}, at {DateTime.Now}")
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
        }

        [Command("AddRule")]
        [Description("Add a rule in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.RuleCount)]
        public async Task AddRule(CommandContext ctx, [RemainingText] string rule)
        {
            var completed = await serverService.AddOrRemoveRule(ctx.Guild.Id, rule, true).ConfigureAwait(false);

            if(!completed)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule already exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = "Added Rule",
                    Description = $"Rule added: {rule}",
                    Color = ModuleInfo.Color
                }).ConfigureAwait(false);
        }

        [Command("RemoveRule")]
        [Description("Add a rule in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.RuleCount)]
        public async Task RemoveRule(CommandContext ctx, int index)
        {
            var rule = await serverService.GetRule(ctx.Guild.Id, index - 1);
            if(rule == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule doesn't exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            await serverService.AddOrRemoveRule(ctx.Guild.Id, rule.RuleContent, false).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Removed Rule",
                Description = $"Rule {index} removed",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }
    }
}
