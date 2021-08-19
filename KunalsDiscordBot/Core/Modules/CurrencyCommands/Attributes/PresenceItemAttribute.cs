using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;

namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PresenceItemAttribute : Attribute
    {
        public PresenceData.PresenceCommand commandNeeded;

        public PresenceItemAttribute(PresenceData.PresenceCommand command) => commandNeeded = command;
    }
}
