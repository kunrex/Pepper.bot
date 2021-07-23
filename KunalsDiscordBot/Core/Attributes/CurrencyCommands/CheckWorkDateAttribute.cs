using System;
namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckWorkDateAttribute : Attribute
    {
        public CheckWorkDateAttribute()
        {
        }
    }
}
