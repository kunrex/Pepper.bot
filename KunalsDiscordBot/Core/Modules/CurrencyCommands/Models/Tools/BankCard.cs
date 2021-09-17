using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;
using DSharpPlus;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Tools
{
    public sealed class BankCard : Tool, IValueModel
    {
        private readonly int minimumIncrease;
        public int MinimumIncrease => minimumIncrease;

        private readonly int maximumIncrease;
        public int MaximumIncrease => maximumIncrease;

        public BankCard(string name, int minimum, int maximum) : base(name)
        {
            minimumIncrease = minimum;
            maximumIncrease = maximum;
        }

        public async override Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            var profile = await service.GetProfile(member.Id, "");

            return await Use(profile, service);
        }

        public async override Task<UseResult> Use(Profile profile, IProfileService profileService)
        {
            var coins = ((IValueModel)this).GetIncrease();

            await profileService.ModifyProfile(profile, x => x.CoinsBankMax += coins);
            return new UseResult
            {
                UseComplete = true,
                Message = $"Bank space increased by {coins}"
            };
        }
    }
}
