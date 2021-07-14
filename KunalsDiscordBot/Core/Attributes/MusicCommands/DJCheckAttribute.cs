using System;
namespace KunalsDiscordBot.Core.Attributes.MusicCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DJCheckAttribute : Attribute
    {
        public DJCheckAttribute()
        {
        }
    }
}
