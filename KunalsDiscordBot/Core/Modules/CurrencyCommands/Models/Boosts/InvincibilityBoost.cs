using System;
using System.Threading.Tasks;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts
{
    public class InvincibilityBoost : Boost, IValueModel, IUsableModel
    {
        private readonly int miminumBoost;
        public int MinimumBoost => miminumBoost;

        private readonly int maximumBoost;
        public int MaximumBoost => maximumBoost;

        private readonly TimeSpan minimumTimeSpan;
        public TimeSpan MinimumTimeSpan => minimumTimeSpan;

        private readonly TimeSpan maximumTimeSpam;
        public TimeSpan MaximumTimeSpam => maximumTimeSpam;

        public InvincibilityBoost(string _name, int _minimumBoost, int _maximumBoost, TimeSpan _minimumTimeSpan, TimeSpan _maximumTimeSpan) : base(_name)
        {
            miminumBoost = _minimumBoost;
            maximumBoost = _maximumBoost;

            minimumTimeSpan = _minimumTimeSpan;
            maximumTimeSpam = _maximumTimeSpan;
        }

        public InvincibilityBoost(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) : base(_name, _percentageIncrease, _span, _start)
        {

        }

        protected override Boost CreateClone(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) => new InvincibilityBoost(_name, _percentageIncrease, _span, _start);

        public int GetBoostPrecentage() => throw new NotImplementedException();
        public TimeSpan GetBoostTimeSpan() => throw new NotImplementedException();

        public async Task<UseResult> Use(Profile userProfile, IProfileService profileService)
        {
            await profileService.AddOrRemoveBoost(userProfile, Name, default, default, default, -1);

            return new UseResult
            {
                useComplete = true,
                message = string.Empty
            };
        }
    }
}
