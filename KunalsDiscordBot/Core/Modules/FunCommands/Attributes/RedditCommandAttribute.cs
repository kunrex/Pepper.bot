using System;

namespace KunalsDiscordBot.Core.Attributes.FunCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RedditCommandAttribute : Attribute
    {
        public RedditCommandAttribute()
        {
        }
    }
}
