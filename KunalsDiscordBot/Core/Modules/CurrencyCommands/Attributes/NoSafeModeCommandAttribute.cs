using System;
namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoSafeModeCommandAttribute : Attribute
    {
        public NoSafeModeCommandAttribute()
        {
        }
    }
}
