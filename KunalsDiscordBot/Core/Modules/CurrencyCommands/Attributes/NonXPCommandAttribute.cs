using System;

namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NonXPCommandAttribute : Attribute
    {
        public NonXPCommandAttribute()
        {
        }
    }
}
