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
    }
}
