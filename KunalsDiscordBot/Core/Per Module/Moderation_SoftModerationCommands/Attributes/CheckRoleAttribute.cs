using System;

namespace KunalsDiscordBot.Core.Attributes.ModerationCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckRoleAttribute : Attribute
    {
        public CheckRoleAttribute()
        {
        }
    }
}
