using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;
using DSharpPlus;
using DiscordBotDataBase.Dal.Models.Profile;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class BoostItem : Item
    {
        public BoostData Data { get; private set; }

        public BoostItem(string name, int price, string description, UseType type, BoostData data, string icon = ":grey_question:") : base(name, price, description, type, icon)
        {
            Data = data;
        }

        public async override Task<UseResult> Use(Profile profile, IProfileService profileService)
        {
            var casted = (ulong)profile.Id;
            if (await profileService.GetBoost(casted, Data.Boost.Name) != null)
                return new UseResult
                {
                    UseComplete = false,
                    Message = $"You already have a {Data.Boost.Name} boost, you can only have 1 boost of one type."
                };

            int boost = Data.GetBoost();
            TimeSpan time = Data.GetBoostTime();

            await profileService.AddOrRemoveBoost(casted, Data.Boost.Name, boost, time, DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"), 1);
            await profileService.AddOrRemoveItem(casted, Name, -1).ConfigureAwait(false);

            return new UseResult
            {
                UseComplete = true,
                Message = $"You got a {boost}% increase in {Data.Boost.Name} for {time.TotalHours} hours"
            };
        }
    }
}
