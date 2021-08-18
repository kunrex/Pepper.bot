using System;

namespace KunalsDiscordBot.Core.Attributes.ModerationCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckMuteRoleAttribute : Attribute
    {
        public CheckMuteRoleAttribute()
        {
        }
    }
}
