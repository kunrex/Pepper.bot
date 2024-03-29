﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.DiscordModels;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Modules.Moderation
{
    [Group("Moderation")]
    [Aliases("Mod")]
    [Decor("Blurple", ":scales:")]
    [Description("Moderation Commands."), ModuleLifespan(ModuleLifespan.Transient)]
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

                try
                {
                    await (await member.CreateDmChannelAsync()).SendMessageAsync($"You have been banned from `{ctx.Guild.Name}`");
                }
                catch
                { }
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

                try
                {
                    await (await member.CreateDmChannelAsync()).SendMessageAsync($"You have been banned from `{ctx.Guild.Name}`");
                }
                catch
                { }
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

                try
                {
                    await (await member.CreateDmChannelAsync()).SendMessageAsync($"You have been kicked from `{ctx.Guild.Name}`");
                }
                catch
                { }
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
        [Description("Gets a ban event using its ID")]
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
             .AddField("Mutes: ", (await modService.GetMutes(member.Id, ctx.Guild.Id)).Count.ToString(), true)
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
            var completed = await modService.AddOrRemoveRule(ctx.Guild.Id, rule, true).ConfigureAwait(false);

            if (!completed)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule already exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
            else
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = "Added Rule",
                    Description = $"Rule added: {rule}",
                    Color = ModuleInfo.Color
                }).ConfigureAwait(false);

                await UpdateRuleMessage(ctx.Guild, ctx.Client);
            }
        }

        [Command("RemoveRule")]
        [Description("Removes a rule in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.RuleCount)]
        public async Task RemoveRule(CommandContext ctx, int index)
        {
            var rule = await modService.GetRule(ctx.Guild.Id, index - 1);
            if(rule == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule doesn't exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            await modService.AddOrRemoveRule(ctx.Guild.Id, rule.RuleContent, false).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Removed Rule",
                Description = $"Rule {index} removed",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

            await UpdateRuleMessage(ctx.Guild, ctx.Client);
        }

        [Command("EditRule")]
        [Description("Removes a custom command in the server")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task EditRule(CommandContext ctx, int index, [RemainingText] string newRule)
        {
            var rule = await modService.GetRule(ctx.Guild.Id, index - 1);
            if (rule == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Rule doesn't exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            await serverService.ModifyData(rule, x => x.RuleContent = newRule);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Edited Rule {index}",
                Description = $"Rule {index} is now `{newRule}`",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

            await UpdateRuleMessage(ctx.Guild, ctx.Client);
        }

        [Command("RemoveAllRules")]
        [Description("Removes all rules in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.RuleCount)]
        public async Task RemoveAllRule(CommandContext ctx)
        {
            var rules = (await modService.GetAllRules(ctx.Guild.Id)).ToArray();

            if(!rules.Any())
            {
                await ctx.RespondAsync("There are no rules in this server");
                return;
            }

            var count = rules.Length;
            foreach (var rule in rules)
                await modService.AddOrRemoveRule(ctx.Guild.Id, rule.RuleContent, false).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Removed All Rules",
                Description = $"{count} rule(s) removed",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

            await UpdateRuleMessage(ctx.Guild, ctx.Client);
        }

        private async Task UpdateRuleMessage(DiscordGuild guild, DiscordClient client)
        {
            var channelID = (ulong)(await serverService.GetServerProfile(guild.Id)).RulesChannelId;
            var channel = guild.GetChannel(channelID);
            if (channel == null)
                return;

            var profile = await serverService.GetModerationData(guild.Id);
            var messageId = (ulong)profile.RulesMessageId;

            try
            {
                var message = await channel.GetMessageAsync(messageId);

                var rules = await modService.GetAllRules(guild.Id);
                if (!rules.Any())
                    await message.DeleteAsync("No rules in server");
                else
                {
                    var embeds = client.GetInteractivity().GetPages(rules, x => (string.Empty, x.RuleContent), new EmbedSkeleton
                    {
                        Color = ModuleInfo.Color
                    }).Select(x => x.Embed);

                    var builder = new DiscordMessageBuilder().WithContent("**READ THE RULES**")
                        .AddEmbeds(embeds);

                    await message.ModifyAsync(builder);
                }
            }
            catch { }
        }

        [Command("CreateRuleMessage")]
        [Description("Creats a dynamic rule message in the rule channel assigned")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task CreateRuleMessage(CommandContext ctx)
        {
            var channelID = (ulong)(await serverService.GetServerProfile(ctx.Guild.Id)).RulesChannelId;
            var channel = ctx.Guild.GetChannel(channelID);
            if(channel == null)
            {
                await ctx.RespondAsync("A valid rule channel nees to be assigned to create a rule message. You can do so using the `general rulechannel` command");
                return;
            }

            var profile = await serverService.GetModerationData(ctx.Guild.Id);
            var messageId = (ulong)profile.RulesMessageId;

            try
            {
                if (await channel.GetMessageAsync(messageId) != null)
                {
                    await ctx.RespondAsync("There already is a rule message for this server");
                    return;
                }
            }
            catch
            {
                var rules = await modService.GetAllRules(ctx.Guild.Id);
                var embeds = ctx.Client.GetInteractivity().GetPages(rules, x => (string.Empty, x.RuleContent), new EmbedSkeleton
                {
                    Color = ModuleInfo.Color
                }).Select(x => x.Embed);

                var builder = new DiscordMessageBuilder().WithContent("**READ THE RULES**")
                    .AddEmbeds(embeds);

                messageId = (await builder.SendAsync(channel)).Id;
                await serverService.ModifyData(profile, x => x.RulesMessageId = (long)messageId);

                await ctx.RespondAsync($"Rule message created in {channel.Mention}");
            }                  
        }

        [Command("DeleteRuleMessage")]
        [Description("Deletes the rule message")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteRuleMessage(CommandContext ctx)
        {
            var channelID = (ulong)(await serverService.GetServerProfile(ctx.Guild.Id)).RulesChannelId;
            var channel = ctx.Guild.GetChannel(channelID);
            if (channel == null)
            {
                await ctx.RespondAsync("A valid rule channel nees to be assigned to create a rule message. You can do so using the `general rulechannel` command");
                return;
            }

            try
            {
                var message = await channel.GetMessageAsync((ulong)(await serverService.GetModerationData(ctx.Guild.Id)).RulesMessageId);
                await message.DeleteAsync("Rule message deletion");

                await ctx.RespondAsync("Rule message deleted");
            }
            catch
            {
                await ctx.RespondAsync("There already is no rule message for this server");
            }
        }

        [Command("AddCustomCommand")]
        [Description("Add a custom command in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.CustomCommandCount)]
        public async Task AddCustomCommant(CommandContext ctx, string commandName, [RemainingText] string commandContent)
        {
            var completed = await modService.AddOrRemoveCustomCommand(ctx.Guild.Id, commandName, true, commandContent).ConfigureAwait(false);

            if (!completed)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Custom Command already exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = "Added Custom Command",
                    Description = $"**{commandName}**: `{commandContent}`",
                    Color = ModuleInfo.Color
                }).ConfigureAwait(false);
        }

        [Command("RemoveCustomCommand")]
        [Description("Removes a custom command in the server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.CustomCommandCount)]
        public async Task RemoveCustomCommand(CommandContext ctx, string name)
        {
            var customCommand = await modService.GetCustomCommand(ctx.Guild.Id, name);
            if (customCommand == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Custom Command doesn't exist",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            await modService.AddOrRemoveCustomCommand(ctx.Guild.Id, name, false).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Removed Custom Command",
                Description = $"Custom Command `{name}` removed",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("EditCustomCommand")]
        [Description("Edits a custom command in the server")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task EditCustomCommand(CommandContext ctx, string name, [RemainingText] string newContent)
        {
            var customCommand = await modService.GetCustomCommand(ctx.Guild.Id, name);
            if (customCommand == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Custom Command doesn't exist",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            await serverService.ModifyData(customCommand, x => x.CommandContent = newContent);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Custom Command",
                Description = $"**{name}** now replies with `{newContent}`",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("AddFilteredWord")]
        [Description("Add a custom command in the server")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddFilteredWord(CommandContext ctx, string word, bool addInfractionOnuse = false)
        {
            var completed = await modService.AddOrRemoveFilteredWord(ctx.Guild.Id, word, addInfractionOnuse, true).ConfigureAwait(false);

            if (!completed)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Filtered word already exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = "Added filtered word",
                    Description = $"||{word}||",
                    Color = ModuleInfo.Color
                }).ConfigureAwait(false);
        }

        [Command("RemoveFilteredWord")]
        [Description("Removes a custom command in the server")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveFilteredWord(CommandContext ctx, string wordToRemove)
        {
            var word = await modService.GetFilteredWord(ctx.Guild.Id, wordToRemove);
            if (word == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"Filtered word already exists",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                return;
            }

            await modService.AddOrRemoveFilteredWord(ctx.Guild.Id, wordToRemove, false, false).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Removed Filtered Word",
                Description = $"||`{wordToRemove}`|| removed from filtered words list",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }
    }
}
