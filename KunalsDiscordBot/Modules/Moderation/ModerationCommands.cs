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

namespace KunalsDiscordBot.Modules.Moderation
{
    [Group("Moderation")]
    [Aliases("Mod")]
    [Decor("DarkButNotBlack", ":gear:")]
    [Description("The user and the bot requires administration roles to run commands in this module")]
    [RequireUserPermissions(Permissions.Administrator)]
    [RequireBotPermissions(Permissions.Administrator | Permissions.BanMembers | Permissions.KickMembers | Permissions.ManageRoles)]
    public class ModerationCommands : BaseCommandModule
    {
        [Command("AddRole")]
        [Aliases("ar")]
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
                Color = DiscordColor.DarkButNotBlack
            };

            embed.AddField("Role: ", role.Mention);
            embed.AddField("From: ", member.Mention);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("RemoveRole")]
        [Aliases("rr")]
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
                Color = DiscordColor.DarkButNotBlack
            };

            embed.AddField("Role: ", role.Mention);
            embed.AddField("To: ", member.Mention);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Ban")]
        public async Task BanMember(CommandContext ctx, DiscordMember member, int numOfDays = 5, string reason = "Unspecified")
        {
            await member.BanAsync(5, reason).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Banned member {member.Username}",
                Color = DiscordColor.DarkButNotBlack
            };

            embed.AddField("Days: ", numOfDays.ToString());
            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Kick")]
        public async Task KickMember(CommandContext ctx, DiscordMember member, string reason = "Unspecified")
        {
            await member.RemoveAsync(reason).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Kicked member {member.Username}",
                Color = DiscordColor.DarkButNotBlack
            };

            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }


        [Command("RemoveAllRoles")]
        [Aliases("rar")]
        public async Task RemoveAllRoles(CommandContext ctx, DiscordMember member, string reason = "Unspecified")
        {
            foreach(var role in member.Roles)
                await member.RevokeRoleAsync(role).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Removed all roles",
            };

            embed.AddField("From: ", member.Mention);
            embed.AddField("Reason: ", reason);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }
    }
}
