using System;

namespace KunalsDiscordBot.Core.Attributes.GameCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireConnect4ChannelAttribute : Attribute
    {
        public RequireConnect4ChannelAttribute()
        {
        }
    }
}
