using System;
using KunalsDiscordBot.Modules.Currency.Shops.Items;

namespace KunalsDiscordBot.Modules.Currency.Shops
{
    public enum UseType
    {
        Presence,
        Invincibility,
        Decoration,
        BoostLuck,
        BoostMoney,
        Offensive,
        Defensive,
        NonBuyable
    }

    public class Item
    {
        public readonly string Name;
        public readonly int Price;
        public readonly int SellingPrice;

        public readonly string Description;

        public UseType Type;

        public PresenceItem? PresenceData { get; private set; }

        public Item(string name, int price, string description, UseType type, PresenceItem? presenceItem = null)
        {
            Name = name;
            Description = description;
            Price = price;
            Type = type;

            SellingPrice = Price / 2;
            PresenceData = presenceItem;
        }

        public virtual void Use() { }
    }
}
