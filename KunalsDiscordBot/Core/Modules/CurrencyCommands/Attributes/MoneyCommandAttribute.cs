using System;

namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MoneyCommandAttribute : Attribute
    {
        public MoneyCommandAttribute()
        {
        }
    }
}
