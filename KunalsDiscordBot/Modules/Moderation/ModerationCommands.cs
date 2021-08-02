//System name spaces
using System.Threading.Tasks;
using System.Linq;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Events;
using System;

using System.Reflection;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Extensions;

namespace KunalsDiscordBot.Modules.Moderation
{
    [Group("Moderation")]
    [Aliases("Mod")]
    [Decor("Blurple", ":scales:")]
    [Description("The user and the bot requires administration roles to run commands in this module")]
    [RequireBotPermissions(Permissions.Administrator)]
    public class ModerationCommands : BaseCommandModule
    {
        private readonly IModerationService modService;
        private readonly IServerService serverService;

        public ModerationCommands(IModerationService moderationService, IServerService _serverService)
        {
            modService = moderationService;
            serverService = _serverService;
        }
        private static readonly DiscordColor Color = typeof(ModerationCommands).GetCustomAttribute<DecorAttribute>().color;
        private static readonly int ThumbnailSize = 30;

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
                    Description = $"The mute role {role.Mention}, is higher than my higher role. Thus I cannot add or remove it.",
                    Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"),
                    Color = Color
                }).ConfigureAwait(false);

                return;
            }

            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("Member already has the specified role");
                return;
            }

            await member.GrantRoleAsync(role).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Added Role",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"),
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, ThumbnailSize)
            }.AddField("Role: ", role.Mention).AddField("From: ", member.Mention);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
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
                    Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"),
                    Color = Color
                }).ConfigureAwait(false);

                return;
            }

            if (member.Roles.First(x => x.Id == role.Id) == null)
            {
                await ctx.RespondAsync("Member does not have the specified role");
                return;
            }

            await member.RevokeRoleAsync(role).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Removed Role",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"),
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, ThumbnailSize)
            }.AddField("Role: ", role.Mention).AddField("To: ", member.Mention);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Ban")]
        [Description("Bans a member")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task BanMember(CommandContext ctx, DiscordMember member, TimeSpan span, [RemainingText] string reason = "Unspecified")
        {
            try
            {
                await member.BanAsync(5, reason).ConfigureAwait(false);
                int id = await modService.AddBan(member.Id, ctx.Guild.Id, ctx.Member.Id, reason, span.ToString());

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = $"Banned member {member.Username} [Id: {id}]",
                    Color = Color,
                    Footer =BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"),
                    Thumbnail = BotService.GetEmbedThumbnail(member, ThumbnailSize)
                }.AddField("Time: ", $"{span.Days} Days, {span.Hours} Hours, {span.Seconds} Seconds")
                 .AddField("Reason: ", reason)).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot ban specified member.\nThis may be because the member is a moderator or administrator").ConfigureAwait(false);
            }
        }

        [Command("Unban")]
        [Description("Unbans a member")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task UnBanMember(CommandContext ctx, DiscordUser user,[RemainingText] string reason = "Unspecified")
        {
            if(await ctx.Guild.GetMemberAsync(user.Id) != null)
            {
                await ctx.RespondAsync("Member is in in the server?");
                return;
            }
            if ((await ctx.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == user.Id) == null)
            {
                await ctx.RespondAsync("Member isn't banned?");
                return;
            }

            await ctx.Guild.UnbanMemberAsync(user, reason).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Unbanned user {user.Username} #{user.Discriminator}",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"),
                Thumbnail = BotService.GetEmbedThumbnail(user, ThumbnailSize)
            }).ConfigureAwait(false);
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

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Kicked member {member.Username} [Id: {id}]",
                    Color = Color,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"
                    },
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = member.AvatarUrl,
                        Height = ThumbnailSize,
                        Width = ThumbnailSize
                    }
                };

                embed.AddField("Reason: ", reason);

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot kick specified member.\nThis may be because the member is a modertaor or administrator").ConfigureAwait(false);
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

            var profile = await modService.GetModerationProfile(kick.ModerationProfileId);

            if ((ulong)profile.GuildId != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Kick with this Id doesn't exist exist in this server");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Kick {kick.Id}",
                Description = $"User: <@{(ulong)profile.DiscordId}>\nReason: {kick.Reason}",
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)kick.ModeratorID).ConfigureAwait(false)).Nickname}"
                }
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
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

            var profile = await modService.GetModerationProfile(ban.ModerationProfileId);

            if ((ulong)profile.GuildId != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Ban with this Id doesn't exist exist in this server");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Ban {ban.Id}",
                Description = $"User: <@{(ulong)profile.DiscordId}>\nReason: {ban.Reason}",
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)ban.ModeratorID).ConfigureAwait(false)).Nickname}"
                }
            };

            var span = TimeSpan.Parse(ban.Time);
            embed.AddField("Time: ", $"{span.Days} Days, {span.Hours} Hours, {span.Seconds} Seconds");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("RemoveAllRoles")]
        [Aliases("rar")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveAllRoles(CommandContext ctx, DiscordMember member, string reason = "Unspecified")
        {
            int botHighest = (await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).GetHighest();
            foreach (var role in member.Roles)
            {
                if (role.Position >= botHighest)
                {
                    await ctx.Channel.SendMessageAsync($"Cannot remove the role {role.Mention}, as its higher than my highest role");
                    continue;
                }

                await member.RevokeRoleAsync(role).ConfigureAwait(false);
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Removed all roles (that I can)",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}")
            }.AddField("From: ", member.Mention)
             .AddField("Reason: ", reason)).ConfigureAwait(false);
        }

        [Command("MemberInfo")]
        [Description("Gets moderation info about the member")]
        public async Task MemberInfo(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = member.Nickname == null ? $"{member.Username} (#{member.Discriminator})" : $"{member.Nickname} (#{member.Username} {member.Discriminator})",
                Thumbnail = BotService.GetEmbedThumbnail(member, ThumbnailSize),
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Member info: {member.Username} #{member.Discriminator}")
            }.AddField("Infractions: ", (await modService.GetInfractions(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .AddField("Endorsements: ", (await modService.GetEndorsements(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .AddField("** **", "** **")
             .AddField("Bans: ", (await modService.GetBans(member.Id, ctx.Guild.Id)).Count.ToString(), true)
             .AddField("Kicks: ", (await modService.GetKicks(member.Id, ctx.Guild.Id)).Count.ToString(), true)).ConfigureAwait(false);
        }

        [Command("AddRule")]
        [Description("Add a rule in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigData.RuleCount)]
        public async Task AddRule(CommandContext ctx, [RemainingText] string rule)
        {
            var completed = await serverService.AddOrRemoveRule(ctx.Guild.Id, rule, true).ConfigureAwait(false);

            if(!completed)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule already exists",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"
                    },
                    Color = Color
                }).ConfigureAwait(false);

                return;
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Added Rule",
                Description = $"Rule added: {rule}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"
                },
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("RemoveRule")]
        [Description("Add a rule in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigData.RuleCount)]
        public async Task RemoveRule(CommandContext ctx, int index)
        {
            var rule = await serverService.GetRule(ctx.Guild.Id, index - 1);
            if(rule == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule doesn't exists",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"
                    },
                    Color = Color
                }).ConfigureAwait(false);

                return;
            }

            await serverService.AddOrRemoveRule(ctx.Guild.Id, rule.RuleContent, false).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Removed Rule",
                Description = $"Rule {index} removed",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"
                },
                Color = Color
            }).ConfigureAwait(false);
        }


        [Command("RuleChannel")]
        [Description("Assigns the rule channel for a server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigData.RuleChannel)]
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
