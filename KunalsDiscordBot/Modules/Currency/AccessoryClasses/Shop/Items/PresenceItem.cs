using System;
using KunalsDiscordBot.Modules.Currency.Shops.Items;

namespace KunalsDiscordBot.Modules.Currency.Shops
{
    public class PresenceItem : Item
    {
        public PresenceItemData Data { get; private set; }

        public PresenceItem(string name, int price, string description, UseType type, PresenceItemData data) : base(name, price, description, type)
        {
            Name = name;
            Price = price;
            Description = description;

            Type = type;
            SellingPrice = Price / 2;
            Data = data;
        }

        public override UseResult Use() => new UseResult { usableItem = false, message = "You can't use this item??" };
    }
}
