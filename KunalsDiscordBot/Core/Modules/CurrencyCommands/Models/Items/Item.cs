using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;
using DSharpPlus;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shop;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    [Flags]
    public enum UseType
    {
        Presence = 1,
        Decoration = 2,
        Boost = 4,
        Tool = 8,
        Offensive = 16,
        Defensive = 32,
        NonBuyable = 64
    }

    public abstract class Item : IDiscordUseableModel
    {
        public string Name { get; protected set; }
        public string EmojiIcon { get; protected set; }

        protected int price;
        public int Price { get => price + StockMarket.Instance.CalculatePrice(price); }

        protected int sellingPrice;
        public int SellingPrice { get => sellingPrice - StockMarket.Instance.CalculatePrice(sellingPrice); }

        public string Description { get; protected set; }

        public UseType UseType { get; protected set; }

        public Item(string name, int _price, string description, UseType type, string icon)
        {
            Name = name;
            price = _price;
            Description = description;

            UseType = type;
            sellingPrice = price / 2;

            EmojiIcon = icon;
        }

        public virtual Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member) => Task.FromResult(new UseResult { UseComplete = true, Message = "" });
        public virtual Task<UseResult> Use(Profile profile, IProfileService profileService) => Task.FromResult(new UseResult { UseComplete = true, Message = "" });
    }
}
