using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Services.General;

namespace KunalsDiscordBot.Core.Attributes.ModerationCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ModeratorNeededAttribute : CheckBaseAttribute
    {
        public ModeratorNeededAttribute()
        {
        }

        public async override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if ((ctx.Member.PermissionsIn(ctx.Channel) & Permissions.Administrator) != Permissions.Administrator)
            {
                var serverService = (IServerService)ctx.Services.GetService(typeof(IServerService));
                var profile = await serverService.GetModerationData(ctx.Guild.Id);

                if (profile.ModeratorRoleId == 0)
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                        {
                            Description = "Note: This server does not have a registered Moderator role"
                        }).ConfigureAwait(false);

                else if (ctx.Member.Roles.FirstOrDefault(x => x.Id == (ulong)profile.ModeratorRoleId) == null)
                    return false;
            }

            return true;
        }
    }
}
