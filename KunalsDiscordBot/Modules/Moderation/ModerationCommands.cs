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
using System.Reflection;

namespace KunalsDiscordBot.Modules.Moderation
{
    [Group("Moderation")]
    [Aliases("Mod")]
    [Decor("DarkButNotBlack", ":gear:")]
    [Description("The user and the bot requires administration roles to run commands in this module")]
    [RequireBotPermissions(Permissions.Administrator)]
    public class ModerationCommands : BaseCommandModule
    {
        private readonly IModerationService service;

        public ModerationCommands(IModerationService moderationService) => service = moderationService;
        private static readonly DiscordColor Color = typeof(ModerationCommands).GetCustomAttribute<Decor>().color;
        private static readonly int ThumbnailSize = 30;

        [Command("AddRole")]
        [Aliases("ar")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AddRole(CommandContext ctx, DiscordRole role, DiscordMember member)
        {
            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("Member already has the specified role");
                return;
            }

            var roleToGrant = ctx.Guild.Roles.First(x => x.Key == role.Id);

            await member.GrantRoleAsync(roleToGrant.Value).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Removed role",
                Color = Color
            };

            embed.AddField("Role: ", role.Mention);
            embed.AddField("From: ", member.Mention);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("RemoveRole")]
        [Aliases("rr")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveRole(CommandContext ctx, DiscordRole role, DiscordMember member)
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
                Color = Color
            };

            embed.AddField("Role: ", role.Mention);
            embed.AddField("To: ", member.Mention);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Ban")]
        [Description("Bans a member")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task BanMember(CommandContext ctx, DiscordMember member, int numOfDays = 5, string reason = "Unspecified")
        {
            await member.BanAsync(5, reason).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Banned member {member.Username}",
                Color = Color
            };

            embed.AddField("Days: ", numOfDays.ToString());
            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            await service.AddBan(member.Id, ctx.Guild.Id, reason, $"{numOfDays} days");
        }

        [Command("Kick")]
        [Description("Kicks a member")]
        [RequireBotPermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task KickMember(CommandContext ctx, DiscordMember member, string reason = "Unspecified")
        {
            await member.RemoveAsync(reason).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Kicked member {member.Username}",
                Color = Color
            };

            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            await service.AddKick(member.Id, ctx.Guild.Id, reason);
        }

        [Command("GetKick")]
        [Description("Gets a kick event using its ID")]
        public async Task GetKick(CommandContext ctx, int kickID)
        {
            var kick = await service.GetKick(kickID);

            if (kick == null)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist");
                return;
            }

            var profile = await service.GetProfile(kick.ModerationProfileId);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Kick {kick.Id}",
                Description = $"User: <@{profile.DiscordId}>\nReason: {kick.Reason}",
                Color = Color
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetBan")]
        [Description("Gets a kick event using its ID")]
        public async Task GetBan(CommandContext ctx, int banID)
        {
            var ban = await service.GetBan(banID);

            if (ban == null)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist");
                return;
            }

            var profile = await service.GetProfile(ban.ModerationProfileId);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Ban {ban.Id}",
                Description = $"User: <@{profile.DiscordId}>\nReason: {ban.Reason}",
                Color = Color
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("RemoveAllRoles")]
        [Aliases("rar")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveAllRoles(CommandContext ctx, DiscordMember member, string reason = "Unspecified")
        {
            foreach(var role in member.Roles)
                await member.RevokeRoleAsync(role).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Removed all roles",
                Color = Color
            };

            embed.AddField("From: ", member.Mention);
            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("MemberInfo")]
        [Description("Gets moderation info about the member")]
        public async Task MembedInfo(CommandContext ctx, DiscordMember member)
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
                Color = Color
            };

            int infractions = await service.GetInfractions(member.Id, ctx.Guild.Id);
            int endorsements = await service.GetEndorsements(member.Id, ctx.Guild.Id);
            int bans = await service.GetBans(member.Id, ctx.Guild.Id);
            int kicks = await service.GetKicks(member.Id, ctx.Guild.Id);

            embed.AddField("Infractions: ", infractions.ToString(), true);
            embed.AddField("Endorsements: ", endorsements.ToString(), true);
            embed.AddField("Bans: ", bans.ToString());
            embed.AddField("Kicks: ", kicks.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }
    }
}
