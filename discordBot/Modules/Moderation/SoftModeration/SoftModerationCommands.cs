//System name spaces
using System.Threading.Tasks;
using System.Linq;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Attributes;

namespace KunalsDiscordBot.Modules.Moderation.SoftModeration
{
    [Group("SoftModeration")]
    [Aliases("softmod", "sm")]
    [Decor("NotQuiteBlack", ":gear:")]
    [Description("Commands for soft moderation, user and bot should be able to manage nicknames")]
    [RequirePermissions(Permissions.ManageNicknames)]
    public class SoftModerationCommands : BaseCommandModule
    {
        [Command("ChangeNickName")]
        [Aliases("cnn")]
        public async Task ChangeNickName(CommandContext ctx, DiscordMember member, string newNick)
        {
            await member.ModifyAsync((DSharpPlus.Net.Models.MemberEditModel obj) => obj.Nickname = newNick);

            await ctx.Channel.SendMessageAsync($"Changed nickname for {member.Username} to {member.Nickname}");
        }

        [Command("VCMute")]
        [Aliases("vcm")]
        public async Task VoiceMuteMember(CommandContext ctx, DiscordMember member, bool toMute)
        {
            await member.SetMuteAsync(toMute).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"Muted {member.Nickname} in the voice channels");
        }

        [Command("VCDeafen")]
        [Aliases("vcd")]
        public async Task VoiceDeafenMember(CommandContext ctx, DiscordMember member, bool toDeafen)
        {
            await member.SetDeafAsync(toDeafen).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"Defeaned {member.Nickname} in the voice channels");
        }
    }
}
