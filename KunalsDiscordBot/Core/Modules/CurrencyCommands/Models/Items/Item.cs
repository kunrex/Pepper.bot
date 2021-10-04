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
        Presence = 0,
        Decoration = 1,
        Boost = 2,
        Tool = 4,
        Offensive = 8,
        Defensive = 16,
        NonBuyable = 32
    }

    public abstract class Item : IDiscordUseableModel
    {
        public string Name { get; protected set; }

        protected int price;
        public int Price { get { Console.WriteLine(StockMarket.Instance.CalculatePrice(price)); return price + StockMarket.Instance.CalculatePrice(price); } }

        protected int sellingPrice;
        public int SellingPrice { get => sellingPrice - StockMarket.Instance.CalculatePrice(sellingPrice); }

        public string Description { get; protected set; }

        public UseType UseType { get; protected set; }


        public Item(string name, int _price, string description, UseType type)
        {
            Name = name;
            price = _price;
            Description = description;

            UseType = type;
            sellingPrice = price / 2;
        }

        public virtual Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member) => Task.FromResult(new UseResult { UseComplete = true, Message = "" });
        public virtual Task<UseResult> Use(Profile profile, IProfileService profileService) => Task.FromResult(new UseResult { UseComplete = true, Message = "" });
    }
}
