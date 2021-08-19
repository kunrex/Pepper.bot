using System;

namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireJobAttribute : Attribute
    {
        public bool require { get; set; } = true;

        public RequireJobAttribute()
        {
        }

        public RequireJobAttribute(bool req) => require = req;
    }
}
