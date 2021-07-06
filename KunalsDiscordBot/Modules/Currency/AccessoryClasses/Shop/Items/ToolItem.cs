using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Modules.Currency.Shops.Items
{
    public class ToolItem : Item
    {
        public ToolData Data;

        public ToolItem(string name, int price, string description, UseType type, ToolData data) :base(name, price, description, type)
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
            int boost = Data.GetBoost();

            switch(Data.Type)
            {
                 case ToolData.ToolType.BankSpace:
                     await service.ChangeCoinsBank(member.Id, boost);
                     break;
            }

            return new UseResult
            {
                useComplete = true,
                message = $"{member.Mention}, Increase {Data.Type} by {boost}"
            };
        }
    }
}
