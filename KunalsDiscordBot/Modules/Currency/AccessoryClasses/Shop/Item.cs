using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Currency.Shops.Items;
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Modules.Currency.Shops
{
    [Flags]
    public enum UseType
    {
        Presence = 0,
        Invincibility = 1,
        Decoration = 2,
        BoostLuck = 4,
        BoostMoney = 8,
        Offensive = 16,
        Defensive = 32,
        NonBuyable = 64
    }

    public abstract class Item
    {
        public string Name { get; protected set; }
        public int Price { get; protected set; }
        public int SellingPrice { get; protected set; }

        public string Description { get; protected set; }

        public UseType Type { get; protected set; }


        public Item(string name, int price, string description, UseType type)
        {

        }

        public abstract Task<UseResult> Use(IProfileService service, DiscordMember member);
    }
}
