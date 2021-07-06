using System;
using KunalsDiscordBot.Core.Exceptions.CurrencyCommands;
using KunalsDiscordBot.Modules.Currency.Shops;
using KunalsDiscordBot.Modules.Currency.Shops.Items;

namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PresenceItemAttribute : Attribute
    {
        public PresenceData.PresenceCommand commandNeeded;

        public PresenceItemAttribute(PresenceData.PresenceCommand command) => commandNeeded = command;
    }
}
