using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using KunalsDiscordBot.Services.General;
using DSharpPlus.CommandsNext.Exceptions;
using KunalsDiscordBot.Core.Exceptions;
using System.Linq;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Attributes.ModerationCommands
{
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
                var profile = await serverService.GetServerProfile(ctx.Guild.Id);

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
