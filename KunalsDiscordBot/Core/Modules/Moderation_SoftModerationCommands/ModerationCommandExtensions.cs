using System;
using System.Linq;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
    {
        public static int GetHighest(this DiscordMember member) => member.Roles.ToList().Select(x => x.Position ).Max();
    }
}
