using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class PresenceItem : Item
    {
        public PresenceData Data { get; private set; }
         
        public PresenceItem(string name, int price, string description, UseType type, PresenceData data) : base(name, price, description, type)
        {
            Data = data;
        }
    }
}
