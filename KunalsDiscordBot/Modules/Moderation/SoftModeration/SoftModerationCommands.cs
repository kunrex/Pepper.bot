//System name spaces
using System.Threading.Tasks;
using System.Linq;

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
    [Decor("NotQuiteBlack", ":gear:")]
    [Description("Commands for soft moderation, user and bot should be able to manage nicknames")]
    [RequireBotPermissions(Permissions.ManageNicknames)]
    [RequireUserPermissions(Permissions.Administrator)]
    public class SoftModerationCommands : BaseCommandModule
    {
        private readonly IModerationService service;

        public SoftModerationCommands(IModerationService moderationService) => service = moderationService;

        [Command("ChangeNickName")]
        [Aliases("cnn")]
        public async Task ChangeNickName(CommandContext ctx, DiscordMember member, string newNick)
        {
            try
            {
                await member.ModifyAsync((DSharpPlus.Net.Models.MemberEditModel obj) => obj.Nickname = newNick);

                await ctx.Channel.SendMessageAsync($"Changed nickname for {member.Username} to {member.Nickname}");
            }
            catch(Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
            }      
        }

        [Command("VCMute")]
        [Aliases("vcm")]
        public async Task VoiceMuteMember(CommandContext ctx, DiscordMember member, bool toMute)
        {
            await member.SetMuteAsync(toMute).ConfigureAwait(false);

            await ctx.RespondAsync($"{(toMute ? "Unmuted" : "Muted")} {member.Username} in the voice channels");
        }

        [Command("VCDeafen")]
        [Aliases("vcd")]
        public async Task VoiceDeafenMember(CommandContext ctx, DiscordMember member, bool toDeafen)
        {
            await member.SetDeafAsync(toDeafen).ConfigureAwait(false);

            await ctx.RespondAsync($"{(toDeafen ? "Undeafened" : "Deafened")} {member.Username} in the voice channels");
        }

        [Command("AddInfraction")]
        [Aliases("AI")]
        [Description("Adds an infraction for the user")]
        public async Task AddInfraction(CommandContext ctx, DiscordMember member, string reason)
        {
            reason ??= string.Empty;
            var id = await service.AddInfraction(member.Id, ctx.Guild.Id, reason);

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Height = 40,
                Width = 40,
                Url = member.AvatarUrl
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Id: {id}"
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Infraction Added",
                Description = $"Reason: {reason}",
                Thumbnail = thumbnail,
                Color = DiscordColor.NotQuiteBlack,
                Footer = footer
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("AddEndorsement")]
        [Aliases("AE")]
        [Description("Adds an endorsement for the user")]
        public async Task AddEndorsement(CommandContext ctx, DiscordMember member, string reason)
        {
            reason ??= string.Empty;
            var id = await service.AddEndorsement(member.Id, ctx.Guild.Id, reason);

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Height = 40,
                Width = 40,
                Url = member.AvatarUrl
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Id: {id}"
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Endrosement Added",
                Description = $"Reason: {reason}",
                Thumbnail = thumbnail,
                Color = DiscordColor.NotQuiteBlack,
                Footer = footer
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetEndorsement")]
        [Aliases("Endorsement", "endorse")]
        [Description("Gets an infraction using its ID")]
        public async Task GetEndorsement(CommandContext ctx, int endorsementID)
        {
            var endorsement = await service.GetEndorsement(endorsementID);

            if(endorsement == null)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist");
                return;
            }

            var profile = await service.GetProfile(endorsement.ModerationProfileId);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Endorsement {endorsement.Id}",
                Description = $"User: <@{profile.DiscordId}>\nReason: {endorsement.Reason}",
                Color = DiscordColor.NotQuiteBlack
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetInfraction")]
        [Aliases("Infraction", "infr")]
        [Description("Gets an infraction using its ID")]
        public async Task GetInfraction(CommandContext ctx, int infractionID)
        {
            var endorsement = await service.GetInfraction(infractionID);

            if(endorsement == null)
            {
                await ctx.Channel.SendMessageAsync("Infraction with this Id doesn't exist");
                return;
            }

            var profile = await service.GetProfile(endorsement.ModerationProfileId);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Infraction {endorsement.Id}",
                Description = $"User: <@{profile.DiscordId}>\nReason: {endorsement.Reason}",
                Color = DiscordColor.NotQuiteBlack
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("MemberInfo")]
        [Description("Gets moderation info about the member")]
        public async Task MembedInfo(CommandContext ctx, DiscordMember member)
        {
            var profile = await service.GetProfile(member.Id, ctx.Guild.Id);

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = member.AvatarUrl,
                Height = 30,
                Width = 30
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = member.Nickname == null ? $"{member.Username} (#{member.Discriminator})" : $"{member.Nickname} (#{member.Username} {member.Discriminator})",
                Thumbnail = thumbnail,
                Color = DiscordColor.DarkButNotBlack
            };

            int infractions = await service.GetInfractions(member.Id, ctx.Guild.Id);
            int endorsements = await service.GetEndorsements(member.Id, ctx.Guild.Id);

            embed.AddField("Infractions: ", infractions.ToString(), true);
            embed.AddField("Endorsements: ", endorsements.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }
    }
}
