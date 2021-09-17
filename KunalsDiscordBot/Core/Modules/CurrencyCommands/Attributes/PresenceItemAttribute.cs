using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;

namespace KunalsDiscordBot.Core.Attributes.CurrencyCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PresenceItemAttribute : Attribute
    {
        public PresenceData.PresenceCommand commandNeeded;

        public PresenceItemAttribute(PresenceData.PresenceCommand command) => commandNeeded = command;
    }
}
