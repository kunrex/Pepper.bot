using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Currency.Shops.Items;
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Modules.Currency.Shops
{
    public class BoostItem : Item
    {
        public BoostData Data { get; private set; }

        public BoostItem(string name, int price, string description, UseType type, BoostData data) : base(name, price, description, type)
        {
            Name = name;
            Price = price;
            Description = description;

            Type = type;
            SellingPrice = Price / 2;
            Data = data;
        }

        public async override Task<UseResult> Use(IProfileService service, DiscordMember member)
        {
            if(await service.GetBoost(member.Id, Data.Type.ToString()) != null)
                return new UseResult
                {
                    useComplete = false,
                    message = $"You already have a {Data.Type} boost"
                };

            int boost = Data.GetBoost();
            int time = Data.GetBoostTime();

            await service.AddOrRemoveBoost(member.Id, Data.Type.ToString(), boost, time, DateTime.Now.ToString("dddd, dd MMMM yyyy"), 1);

            return new UseResult
            {
                useComplete = true,
                message = $"{member.Mention}, {boost}% increase in {Data.Type} for {time} hours"
            };
        }
    }
}
