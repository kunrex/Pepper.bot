using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;
using DSharpPlus;
using DiscordBotDataBase.Dal.Models.Profile;

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
        public int Price { get; protected set; }
        public int SellingPrice { get; protected set; }

        public string Description { get; protected set; }

        public UseType UseType { get; protected set; }


        public Item(string name, int price, string description, UseType type)
        {
            Name = name;
            Price = price;
            Description = description;

            UseType = type;
            SellingPrice = Price / 2;
        }

        public virtual Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member) => Task.FromResult(new UseResult { UseComplete = true, Message = "" });
        public virtual Task<UseResult> Use(Profile profile, IProfileService profileService) => Task.FromResult(new UseResult { UseComplete = true, Message = "" });
    }
}
