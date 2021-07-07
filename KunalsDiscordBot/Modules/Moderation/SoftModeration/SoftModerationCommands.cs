//System name spaces
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Attributes;
using System;
using KunalsDiscordBot.Modules.Moderation.Services;

namespace KunalsDiscordBot.Modules.Moderation.SoftModeration
{
    [Group("SoftModeration")]
    [Aliases("softmod", "sm")]
    [Decor("Blurple", ":gear:")]
    [Description("Commands for soft moderation, user and bot should be able to manage nicknames")]
    [RequireBotPermissions(Permissions.Administrator)]
    public class SoftModerationCommands : BaseCommandModule
    {
        private readonly IModerationService service;

        public SoftModerationCommands(IModerationService moderationService) => service = moderationService;
        private static readonly DiscordColor Color = typeof(SoftModerationCommands).GetCustomAttribute<Decor>().color;
        private static readonly int ThumbnailSize = 30;

        [Command("ChangeNickName")]
        [Aliases("cnn")]
        [RequireBotPermissions(Permissions.ManageNicknames)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ChangeNickName(CommandContext ctx, DiscordMember member, [RemainingText] string newNick)
        {
            try
            {
                await member.ModifyAsync((DSharpPlus.Net.Models.MemberEditModel obj) => obj.Nickname = newNick);

                await ctx.Channel.SendMessageAsync($"Changed nickname for {member.Username} to {member.Nickname}");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync("Cannot change the nick name of the spcified user.\nThis may be because the specified user is a modertor or administrator").ConfigureAwait(false);
            }      
        }

        [Command("VCMute")]
        [Aliases("vcm")]
        [RequireUserPermissions(Permissions.Administrator)]
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
        [RequireUserPermissions(Permissions.Administrator)]
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
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddInfraction(CommandContext ctx, DiscordMember member, [RemainingText]  string reason = "Unpsecified")
        {
            var id = await service.AddInfraction(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

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
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddEndorsement(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unpsecified")
        {
            var id = await service.AddEndorsement(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

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
            var endorsement = await service.GetEndorsement(endorsementID);

            if(endorsement == null)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist");
                return;
            }

            var profile = await service.GetModerationProfile(endorsement.ModerationProfileId);

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
            var infraction = await service.GetInfraction(infractionID);

            if(infraction == null)
            {
                await ctx.Channel.SendMessageAsync("Infraction with this Id doesn't exist");
                return;
            }

            var profile = await service.GetModerationProfile(infraction.ModerationProfileId);

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
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddRule(CommandContext ctx, int index)
        {
           var rule =  await service.GetRule(ctx.Guild.Id, index - 1).ConfigureAwait(false);

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
    }
}
