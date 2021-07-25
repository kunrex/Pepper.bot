using System;
using System.Linq;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Modules.Moderation
{
    public static class ModerationCommandExtensions
    {
        public static int GetHighest(this DiscordMember member) => member.Roles.ToList().Select(x => x.Position ).Max();
    }
}
