using System;
namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireProfileAttribute : Attribute
    {
        public RequireProfileAttribute()
        {
        }
    }
}
