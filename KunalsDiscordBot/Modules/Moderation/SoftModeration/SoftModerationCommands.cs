//System name spaces
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Attributes;
using System;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Attributes.ModerationCommands;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Exceptions;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Net.Models;
using KunalsDiscordBot.Core.Attributes;
using System.Net.Http;
using System.Drawing;
using KunalsDiscordBot.Events;

namespace KunalsDiscordBot.Modules.Moderation.SoftModeration
{
    [Group("SoftModeration")]
    [Aliases("softmod", "sm")]
    [Decor("Blurple", ":scales:")]
    [Description("Commands for soft moderation, user and bot should be able to manage nicknames")]
    [RequireBotPermissions(Permissions.Administrator)]
    public class SoftModerationCommands : BaseCommandModule
    {
        private readonly IModerationService modService;
        private readonly IServerService serverService;

        public SoftModerationCommands(IModerationService moderationService, IServerService _serverService)
        {
            modService = moderationService;
            serverService = _serverService;
        }

        private static readonly DiscordColor Color = typeof(SoftModerationCommands).GetCustomAttribute<DecorAttribute>().color;
        private static readonly int ThumbnailSize = 30;

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var moderatorNeeded = ctx.Command.CustomAttributes.FirstOrDefault(x => x is ModeratorNeededAttribute) != null;
            if(moderatorNeeded)
            {
                if ((ctx.Member.PermissionsIn(ctx.Channel) & Permissions.Administrator) != Permissions.Administrator)
                {
                    var profile = await serverService.GetServerProfile(ctx.Guild.Id);

                    if (profile.ModeratorRoleId == 0)
                    {
                        await ctx.RespondAsync("This server does not have a registered Moderator role, use the `general`").ConfigureAwait(false);
                        throw new CustomCommandException();
                    }
                    else if (ctx.Member.Roles.FirstOrDefault(x => x.Id == (ulong)profile.ModeratorRoleId) == null)
                        throw new ChecksFailedException(ctx.Command, ctx, new List<CheckBaseAttribute> { });
                }
            }

            var checkMute = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckMuteRoleAttribute) != null;
            if(checkMute)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id);
                var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id == (ulong)profile.MutedRoleId).Value;

                if (role == null)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = $"There is no muted role stored for server: {ctx.Guild.Name}. Use the `soft moderation setmuterole` command to do so",
                        Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"),
                        Color = Color
                    }).ConfigureAwait(false);

                    throw new CustomCommandException();
                }

                var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
                if(botMember.GetHighest() < role.Position)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = $"The mute role {role.Mention}, is higher than my higher role. Thus I cannot add or remove it.",
                        Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"),
                        Color = Color
                    }).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("SetModRole")]
        [Description("Assigns the moderator role for the server. This command can only be ran by an administrator")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigData.ModRole)]
        public async Task SetModRole(CommandContext ctx, DiscordRole role)
        {
            await serverService.SetModeratorRole(ctx.Guild.Id, role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {role.Mention} as the moderator role for the server",
                Footer = BotService.GetEmbedFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }


        [Command("SetMuteRole")]
        [Description("Sets the mute role of a server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigData.MutedRole)]
        public async Task SetMuteRole(CommandContext ctx, DiscordRole role)
        {
            await serverService.SetMuteRoleId(ctx.Guild.Id, role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Muted Role Saved",
                Description = $"Succesfully stored {role.Mention} as the mute role for the server",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Admin: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"
                },
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("ChangeNickName")]
        [Aliases("cnn")]
        [RequireBotPermissions(Permissions.ManageNicknames)]
        [ModeratorNeeded]
        public async Task ChangeNickName(CommandContext ctx, DiscordMember member, [RemainingText] string newNick)
        {
            try
            {
                await member.ModifyAsync((DSharpPlus.Net.Models.MemberEditModel obj) => obj.Nickname = newNick);

                await ctx.Channel.SendMessageAsync($"Changed nickname for {member.Username} to {newNick}");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync("Cannot change the nick name of the spcified user.\nThis may be because the specified user is a modertor or administrator").ConfigureAwait(false);
            }      
        }

        [Command("VCMute")]
        [Aliases("vcm")]
        [ModeratorNeeded]
        public async Task VoiceMuteMember(CommandContext ctx, DiscordMember member, bool toMute)
        {
            try
            {
                await member.SetMuteAsync(toMute).ConfigureAwait(false);

                await ctx.RespondAsync($"{(toMute ? "Unmuted" : "Muted")} {member.Username} in the voice channels");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot {(toMute ? "unmute" : "mute")} the spcified user.\nThis may be because the specified user is a modertor or administrator").ConfigureAwait(false);
            }
        }

        [Command("VCDeafen")]
        [Aliases("vcd")]
        [ModeratorNeeded]
        public async Task VoiceDeafenMember(CommandContext ctx, DiscordMember member, bool toDeafen)
        {
            try
            {
                await member.SetDeafAsync(toDeafen).ConfigureAwait(false);

                await ctx.RespondAsync($"{(toDeafen ? "Undeafened" : "Deafened")} {member.Username} in the voice channels");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Cannot {(toDeafen ? "undeafen" : "deafen")} the spcified user.\nThis may be because the specified user is a modertor or administrator").ConfigureAwait(false);
            }
        }

        [Command("AddInfraction")]
        [Aliases("AI", "Infract")]
        [Description("Adds an infraction for the user")]
        [ModeratorNeeded]
        public async Task AddInfraction(CommandContext ctx, DiscordMember member, [RemainingText]  string reason = "Unpsecified")
        {
            var id = await modService.AddInfraction(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Infraction Added [Id: {id}]",
                Description = $"Reason: {reason}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = member.AvatarUrl
                },
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"
                },
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("AddEndorsement")]
        [Aliases("AE", "Endorse")]
        [Description("Adds an endorsement for the user")]
        [ModeratorNeeded]
        public async Task AddEndorsement(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unpsecified")
        {
            var id = await modService.AddEndorsement(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Endrosement Added [Id: {id}]",
                Description = $"Reason: {reason}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = member.AvatarUrl
                },
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"
                },
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetEndorsement")]
        [Aliases("ge")]
        [Description("Gets an infraction using its ID")]
        public async Task GetEndorsement(CommandContext ctx, int endorsementID)
        {
            var endorsement = await modService.GetEndorsement(endorsementID);

            if(endorsement == null)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist");
                return;
            }

            var profile = await modService.GetModerationProfile(endorsement.ModerationProfileId);

            if((ulong)profile.GuildId != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist exist in this server");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Endorsement {endorsement.Id}",
                Description = $"User: <@{(ulong)profile.DiscordId}>\nReason: {endorsement.Reason}",
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)endorsement.ModeratorID).ConfigureAwait(false)).Nickname}"
                }
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetInfraction")]
        [Aliases("gi")]
        [Description("Gets an infraction using its ID")]
        public async Task GetInfraction(CommandContext ctx, int infractionID)
        {
            var infraction = await modService.GetInfraction(infractionID);

            if(infraction == null)
            {
                await ctx.Channel.SendMessageAsync("Infraction with this Id doesn't exist");
                return;
            }

            var profile = await modService.GetModerationProfile(infraction.ModerationProfileId);

            if ((ulong)profile.GuildId != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Infraction with this Id doesn't exist exist in this server");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Infraction {infraction.Id}",
                Description = $"User: <@{(ulong)profile.DiscordId}>\nReason: {infraction.Reason}",
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator:{(await ctx.Guild.GetMemberAsync((ulong)infraction.ModeratorID).ConfigureAwait(false)).Nickname}"
                }
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Rule")]
        [Description("Displays a rule by its index")]
        public async Task AddRule(CommandContext ctx, int index)
        {
           var rule =  await serverService.GetRule(ctx.Guild.Id, index - 1).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Rule {index}",
                Description = $"{(rule == null ? "Rule doesn't exist" : rule.RuleContent)}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"User: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}"
                },
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("ClearChat")]
        [Aliases("Clear")]
        [Description("Deletes `x` number of messages")]
        [RequireBotPermissions(Permissions.ManageMessages)]
        [ModeratorNeeded]
        public async Task ClearChat(CommandContext ctx, int number)
        {
            foreach (var message in await ctx.Channel.GetMessagesAsync(number))
                await message.DeleteAsync();

            await ctx.Channel.SendMessageAsync("**Chat has been cleaned**").ConfigureAwait(false);
        }

        [Command("Slowmode")]
        [Aliases("slow")]
        [ModeratorNeeded]
        [Description("Sets the slow mode for the chat")]
        [RequireBotPermissions(Permissions.ManageChannels)]
        public async Task SlowMode(CommandContext ctx, int seconds)
        {
            if(ctx.Channel.PerUserRateLimit == seconds)
            {
                await ctx.RespondAsync($"Slow mode for {ctx.Channel.Mention} already is {seconds} seconds?").ConfigureAwait(false);
                return;
            }

            await ctx.Channel.ModifyAsync((ChannelEditModel obj) => obj.PerUserRateLimit = seconds);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Description = $"{(seconds == 0 ? $"Disabled slow mode for {ctx.Channel.Mention}": "$Set Slow Mode for {ctx.Channel.Mention} to {seconds} seconds")}",
                Footer = BotService.GetEmbedFooter($"Mod/Admin: {ctx.Member.DisplayName}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("SetNSFW")]
        [Aliases("NSFW")]
        [ModeratorNeeded]
        [Description("Changes the NSFW status of a channel")]
        [RequireBotPermissions(Permissions.ManageChannels)]
        public async Task NSFW(CommandContext ctx, bool toSet)
        {
            if(ctx.Channel.IsNSFW == toSet)
            {
                await ctx.RespondAsync($"{(toSet ? "Channel already is NSFW?" : "Channel isn't NSFW in the first place?")}");
                return;
            }

            await ctx.Channel.ModifyAsync((ChannelEditModel obj) => obj.Nsfw = toSet);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Description = $"{ctx.Channel.Mention} {(toSet ? "is now NSFW" : "is not NSFW anymore")}",
                Footer = BotService.GetEmbedFooter($"Mod/Admin: {ctx.Member.DisplayName}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("AddEmoji")]
        [Description("Addes an emoji to the server")]
        [ModeratorNeeded]
        [RequireBotPermissions(Permissions.ManageEmojis)]
        public async Task AddEmoji(CommandContext ctx, string name, string url)
        {
            await ctx.Channel.SendMessageAsync("This might take a second").ConfigureAwait(false);

            await using var stream = await new HttpClient().GetStreamAsync(url);
            var emoji = await ctx.Guild.CreateEmojiAsync(name, stream);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Emoji Adeed! :{emoji.Name}:",
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("RemoveEmoji")]
        [Description("Removes an emoji from the server")]
        [ModeratorNeeded]
        [RequireBotPermissions(Permissions.ManageEmojis)]
        public async Task RemoveEmoji(CommandContext ctx, string name)
        {
            await ctx.Channel.SendMessageAsync("This might take a second").ConfigureAwait(false);

            var emoji = ctx.Guild.Emojis.Values.FirstOrDefault(x => x.Name == name);
            if (emoji == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "Emoji not found",
                    Color = Color
                }).ConfigureAwait(false);

                return;
            }

            await ctx.Guild.DeleteEmojiAsync(await ctx.Guild.GetEmojiAsync(emoji.Id));

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Removed Emoji {name}",
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("Mute")]
        [Description("Mutes a member")]
        [ModeratorNeeded, CheckMuteRole]
        public async Task Mute(CommandContext ctx, DiscordMember member, TimeSpan span, [RemainingText] string reason = "Unspecified")
        {
            var profile = await serverService.GetServerProfile(ctx.Guild.Id);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id == (ulong)profile.MutedRoleId).Value;

            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("Member is already muted");
                return;
            }

            await member.GrantRoleAsync(role).ConfigureAwait(false);
            int id = await modService.AddMute(member.Id, ctx.Guild.Id, ctx.Member.Id, reason, span.ToString());
            BotEventFactory.CreateEvent().WithSpan(span).WithEvent((s, e) =>
            {
                if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
                    Task.Run(async () => await member.RevokeRoleAsync(role).ConfigureAwait(false));
            }).Execute();

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Muted Member {member.DisplayName}",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"),
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, ThumbnailSize)
            };

            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Unmute")]
        [Description("Unmutes a member")]
        [ModeratorNeeded, CheckMuteRole]
        public async Task UnMute(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unspecified")
        {
            var profile = await serverService.GetServerProfile(ctx.Guild.Id);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id == (ulong)profile.MutedRoleId).Value;

            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) == null)
            {
                await ctx.Channel.SendMessageAsync("Member isn't muted?");
                return;
            }

            await member.RevokeRoleAsync(role).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Unmuted Member {member.DisplayName}",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}"),
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, ThumbnailSize)
            };

            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetMute")]
        [Description("Gets a mute event using its ID")]
        public async Task GetMute(CommandContext ctx, int muteId)
        {
            var mute = await modService.GetMute(muteId);

            if (mute == null)
            {
                await ctx.Channel.SendMessageAsync("Mute with this Id doesn't exist");
                return;
            }

            var profile = await modService.GetModerationProfile(mute.ModerationProfileId);

            if ((ulong)profile.GuildId != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Mute with this Id doesn't exist exist in this server");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Mute {mute.Id}",
                Description = $"User: <@{(ulong)profile.DiscordId}>\nReason: {mute.Reason}",
                Color = Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)mute.ModeratorID).ConfigureAwait(false)).Nickname}"
                }
            };

            var span = TimeSpan.Parse(mute.Time);
            embed.AddField("Time: ", $"{span.Days} Days, {span.Hours} Hours, {span.Seconds} Seconds");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }
    }
}
