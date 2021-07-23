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
            try
            {
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

                embed.AddField("Role: ", role.Mention);
                embed.AddField("From: ", member.Mention);

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot add the specifed role from specified member.\nThis is because the the highest role I have is not higher than the given role").ConfigureAwait(false);
            }
        }

        [Command("RemoveRole")]
        [Aliases("rr")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveRole(CommandContext ctx, DiscordRole role, DiscordMember member)
        {
            try
            {

                if (member.Roles.First(x => x.Id == role.Id) == null)
                {
                    await ctx.RespondAsync("Member does not have the specified role");
                    return;
                }

                await member.RevokeRoleAsync(role).ConfigureAwait(false);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Removed role",
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

                embed.AddField("Role: ", role.Mention);
                embed.AddField("To: ", member.Mention);

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot remove the specifed role from specified member.\nThis is because the the highest role I have is not higher than the given role").ConfigureAwait(false);
            }
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

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Banned member {member.Username} [Id: {id}]",
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

                embed.AddField("Time: ", $"{span.Days} Days, {span.Hours} Hours, {span.Seconds} Seconds");
                embed.AddField("Reason: ", reason);

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot ban specified member.\nThis may be because the member is a moderator or administrator").ConfigureAwait(false);
            }
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
            try
            {


                foreach (var role in member.Roles)
                    await member.RevokeRoleAsync(role).ConfigureAwait(false);

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Removed all roles",
                    Color = Color,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"
                    }

                };

                embed.AddField("From: ", member.Mention);
                embed.AddField("Reason: ", reason);

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot remove a role from member, this is because one of roles on this memeber is higher than the highest role I have").ConfigureAwait(false);
            }
        }

        [Command("MemberInfo")]
        [Description("Gets moderation info about the member")]
        public async Task MemberInfo(CommandContext ctx, DiscordMember member)
        {
            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = member.AvatarUrl,
                Height = ThumbnailSize,
                Width = ThumbnailSize
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = member.Nickname == null ? $"{member.Username} (#{member.Discriminator})" : $"{member.Nickname} (#{member.Username} {member.Discriminator})",
                Thumbnail = thumbnail,
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Member info: {member.Username} #{member.Discriminator}"
                }
            };

            embed.AddField("Infractions: ", (await modService.GetInfractions(member.Id, ctx.Guild.Id)).Count.ToString(), true);
            embed.AddField("Endorsements: ", (await modService.GetEndorsements(member.Id, ctx.Guild.Id)).Count.ToString(), true);
            embed.AddField("Bans: ", (await modService.GetBans(member.Id, ctx.Guild.Id)).Count.ToString());
            embed.AddField("Kicks: ", (await modService.GetKicks(member.Id, ctx.Guild.Id)).Count.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
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
