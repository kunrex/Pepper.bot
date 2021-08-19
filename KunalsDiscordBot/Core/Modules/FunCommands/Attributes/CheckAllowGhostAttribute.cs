using System;

namespace KunalsDiscordBot.Core.Attributes.FunCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckAllowGhostAttribute : Attribute
    {
        public CheckAllowGhostAttribute()
        {
        }
    }
}
