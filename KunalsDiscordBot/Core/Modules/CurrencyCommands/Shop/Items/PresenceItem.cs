﻿using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops
{
    public class PresenceItem : Item
    {
        public PresenceData Data { get; private set; }
         
        public PresenceItem(string name, int price, string description, UseType type, PresenceData data) : base(name, price, description, type)
        {
            Name = name;
            Price = price;
            Description = description;

            Type = type;
            SellingPrice = Price / 2;
            Data = data;
        }

        public async override Task<UseResult> Use(IProfileService service, DiscordMember member) => new UseResult { useComplete = false, message = "You can't use this item??" };
    }
}