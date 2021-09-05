using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops
{
    public class BoostItem : Item
    {
        public BoostData Data { get; private set; }

        public BoostItem(string name, int price, string description, UseType type, BoostData data) : base(name, price, description, type)
        {
            Data = data;
        }

        public async override Task<UseResult> Use(IProfileService service, DiscordMember member)
        {
            if(await service.GetBoost(member.Id, Data.Boost.Name) != null)
                return new UseResult
                {
                    useComplete = false,
                    message = $"You already have a {Data.Boost.Name} boost, you can only have 1 boost of one type."
                };

            int boost = Data.GetBoost();
            TimeSpan time = Data.GetBoostTime();

            await service.AddOrRemoveBoost(member.Id, Data.Boost.Name, boost, time, DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"), 1);
            await service.AddOrRemoveItem(member.Id, Name, -1).ConfigureAwait(false);

            return new UseResult
            {
                useComplete = true,
                message = $"You got a {boost}% increase in {Data.Boost.Name} for {time.TotalHours} hours"
            };
        }
    }
}
