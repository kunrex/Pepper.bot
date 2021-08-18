using System;

namespace KunalsDiscordBot.Core.Attributes.FunCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckAllowSpamAttribute : Attribute
    {
        public CheckAllowSpamAttribute()
        {
        }
    }
}
