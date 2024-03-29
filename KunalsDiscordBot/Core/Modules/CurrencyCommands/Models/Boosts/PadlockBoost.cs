﻿using System;
using System.Threading.Tasks;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts
{
    public class PadlockBoost : Boost, ITheftProtection, ITimeSpanValueModel
    {
        private readonly int miminumBoost;
        public int MinimumIncrease => miminumBoost;

        private readonly int maximumBoost;
        public int MaximumIncrease => maximumBoost;

        private readonly TimeSpan minimumTimeSpan;
        public TimeSpan MinimumTimeSpan => minimumTimeSpan;

        private readonly TimeSpan maximumTimeSpam;
        public TimeSpan MaximumTimeSpam => maximumTimeSpam;

        public int Order => 1;
        public int MethodId => 1;

        public PadlockBoost(string _name, int _minimumBoost, int _maximumBoost, TimeSpan _minimumTimeSpan, TimeSpan _maximumTimeSpan) : base(_name)
        {
            miminumBoost = _minimumBoost;
            maximumBoost = _maximumBoost;

            minimumTimeSpan = _minimumTimeSpan;
            maximumTimeSpam = _maximumTimeSpan;
        }

        public PadlockBoost(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) : base(_name, _percentageIncrease, _span, _start)
        {
            
        }

        protected override Boost CreateClone(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) => new PadlockBoost(_name, _percentageIncrease, _span, _start);

        public async Task<UseResult> Use(Profile userProfile, IProfileService profileService)
        {
            await profileService.AddOrRemoveBoost(userProfile, Name, 0, TimeSpan.FromSeconds(0), "", - 1);

            return new UseResult
            {
                UseComplete = true,
                Message = $"Well sadly, the person you tried to rob had a padlock on and ya failed <:kek:849534098643877929>"
            };
        }

        public Task<UseResult> Use(Profile userProfile, Profile robberProfile, IProfileService profileService) => throw new NotImplementedException();
    }
}
